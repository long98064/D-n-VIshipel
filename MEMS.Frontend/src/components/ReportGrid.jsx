import React, { useEffect, useState } from 'react';
import { Table, InputNumber, Button, Space, Typography, Tag } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import useReportStore from '../store/useReportStore';
import { useAuthStore } from '../store/useAuthStore'; // Lấy auth để check Role

const { Title } = Typography;

// Component Ô nhập liệu thông minh (chỉ cho phép sửa ở Lá)
const CellInput = ({ value, nodeId, isLeaf }) => {
    const [localValue, setLocalValue] = useState(value);
    const updateLeafValue = useReportStore(state => state.updateLeafValue);

    useEffect(() => {
        setLocalValue(value);
    }, [value]);

    const handleBlur = () => {
        if (localValue !== value) {
            updateLeafValue(nodeId, localValue);
        }
    };

    if (!isLeaf) {
        // Dòng tổng (Cha): In đậm, ReadOnly
        return <span style={{ fontWeight: 'bold', color: '#1677ff', fontSize: '15px' }}>
            {(value || 0).toLocaleString('vi-VN')} VNĐ
        </span>;
    }

    return (
        <InputNumber
            value={localValue}
            onChange={setLocalValue}
            onBlur={handleBlur}
            formatter={(val) => `${val}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
            parser={(val) => val.replace(/\$\s?|(,*)/g, '')}
            style={{ width: '100%' }}
        />
    );
};

const ReportGrid = () => {
    const { categoryTree, isLoading } = useReportStore();
    const { user } = useAuthStore();
    
    // Check quyền Admin để hiển thị tính năng chỉnh sửa Schema
    const isAdmin = user?.role === 'SuperAdmin' || user?.role === 'Admin';

    // Cấu hình Cột động
    const columns = [
        {
            title: 'Hạng mục Dịch vụ',
            dataIndex: 'title',
            key: 'title',
            render: (text, record) => (
                <span style={{ fontWeight: record.isLeaf ? 'normal' : 'bold' }}>
                    {text} {!record.isLeaf && <Tag color="blue" style={{marginLeft: 8}}>Tổng tự động</Tag>}
                </span>
            ),
        },
        {
            title: 'Giá trị (VNĐ)',
            dataIndex: 'totalValue',
            key: 'totalValue',
            width: '30%',
            render: (val, record) => (
                <CellInput value={val} nodeId={record.id} isLeaf={record.isLeaf} />
            ),
        }
    ];

    // GIAI ĐOẠN 5: ÁP DỤNG RBAC & NÚT TĨNH CHỐNG HOVER
    if (isAdmin) {
        columns.push({
            title: 'Thao tác Cấu trúc',
            key: 'action',
            width: '20%',
            align: 'center',
            render: (_, record) => (
                // Các nút hiển thị tĩnh (block/flex), không dùng CSS :hover
                <Space size="middle" style={{ display: 'flex', justifyContent: 'center' }}>
                    <Button size="small" type="primary" ghost icon={<EditOutlined />}>Sửa</Button>
                    <Button size="small" danger ghost icon={<DeleteOutlined />}>Xóa</Button>
                </Space>
            ),
        });
    }

    return (
        <div style={{ padding: 20, background: '#fff', borderRadius: 8 }}>
            <div style={{ marginBottom: 16, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Title level={4} style={{ margin: 0 }}>Nhập liệu Báo cáo Động</Title>
                
                <Space>
                    {/* Bọc RBAC: Chỉ Admin mới thấy nút Thêm mới dịch vụ */}
                    {isAdmin && (
                        <Button type="dashed" icon={<PlusOutlined />} style={{ borderColor: '#1677ff', color: '#1677ff' }}>
                            Thêm mới Dịch vụ (Schema)
                        </Button>
                    )}
                    <Button type="primary">Lưu Báo cáo</Button>
                </Space>
            </div>
            
            <Table 
                columns={columns} 
                dataSource={categoryTree} 
                rowKey="id"
                loading={isLoading}
                pagination={false}
                bordered
                // Ant Design tự động render cây (tree) nếu data có trường 'children'
                rowClassName={(record) => record.isLeaf ? 'row-leaf' : 'row-parent-bold'}
            />
        </div>
    );
};

export default ReportGrid;