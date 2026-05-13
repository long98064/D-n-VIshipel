import React, { useState, useEffect } from 'react';
import { Table, Button, Space, Modal, Form, Input, Switch, message, Popconfirm, Upload, Divider, Alert, Tag } from 'antd';
import { PlusOutlined, DeleteOutlined, UploadOutlined, FileExcelOutlined, ReloadOutlined } from '@ant-design/icons';
import axiosClient from '../../api/axiosClient';

const CategoryManager = () => {
    const [treeData, setTreeData] = useState([]);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [importing, setImporting] = useState(false);
    const [importResult, setImportResult] = useState(null); // Kết quả import Excel
    const [form] = Form.useForm();
    const [parentId, setParentId] = useState(null);

    const fetchTree = async () => {
        try {
            const res = await axiosClient.get('/categories/tree');
            setTreeData(Array.isArray(res) ? res : []);
        } catch (error) {
            message.error("Lỗi khi tải cấu trúc danh mục");
        }
    };

    useEffect(() => { fetchTree(); }, []);

    // ============================================================
    // XỬ LÝ IMPORT FILE EXCEL DANH MỤC
    // ============================================================
    const handleImportExcel = async ({ file }) => {
        setImporting(true);
        setImportResult(null);
        try {
            const formData = new FormData();
            formData.append('file', file);

            // Axios gửi multipart/form-data
            const res = await axiosClient.post('/categories/import-from-excel', formData, {
                headers: { 'Content-Type': 'multipart/form-data' }
            });

            setImportResult({ type: 'success', message: res.message, imported: res.imported });
            message.success(`Import thành công ${res.imported} danh mục!`);
            fetchTree(); // Auto refetch sau khi import
        } catch (error) {
            const errMsg = error?.response?.data?.error || 'Lỗi khi import file Excel';
            setImportResult({ type: 'error', message: errMsg });
            message.error(errMsg);
        } finally {
            setImporting(false);
        }
        // Trả về false để Antd Upload không tự upload
        return false;
    };

    const handleDelete = async (id) => {
        try {
            await axiosClient.delete(`/admin/categories/${id}`);
            message.success("Đã xóa hạng mục (Soft Delete)");
            fetchTree();
        } catch (error) {
            message.error("Lỗi khi xóa");
        }
    };

    const handleAddSubmit = async (values) => {
        try {
            const payload = { ...values, parentId };
            await axiosClient.post('/admin/categories', payload);
            message.success("Thêm mới thành công");
            setIsModalOpen(false);
            form.resetFields();
            fetchTree();
        } catch (error) {
            message.error("Lỗi khi thêm hạng mục");
        }
    };

    const getLevelColor = (code) => {
        if (!code) return 'default';
        if (code.startsWith('CAT_L4')) return 'purple';
        if (code.includes('_') && code.split('_').length >= 4) return 'cyan';
        if (code.includes('_') && code.split('_').length === 3) return 'blue';
        return 'green';
    };

    const columns = [
        { 
            title: 'Mã (Code)', dataIndex: 'code', key: 'code', width: 150,
            render: (val) => <Tag color={getLevelColor(val)}>{val}</Tag>
        },
        { title: 'Tên Hạng Mục', dataIndex: 'title', key: 'title' },
        { 
            title: 'Là Nốt Lá', dataIndex: 'isLeaf', width: 100,
            render: (val) => val ? <Tag color="volcano">Nhập liệu</Tag> : <Tag>Nhóm</Tag>
        },
        { title: 'Thứ tự', dataIndex: 'orderIndex', key: 'orderIndex', width: 80 },
        {
            title: 'Hành động', key: 'action', width: 160,
            render: (_, record) => (
                <Space size="small">
                    <Button 
                        type="link" size="small"
                        onClick={() => {
                            setParentId(record.id);
                            form.setFieldsValue({ isLeaf: true });
                            setIsModalOpen(true);
                        }}
                    >
                        + Con
                    </Button>
                    <Popconfirm title="Chắc chắn xóa?" onConfirm={() => handleDelete(record.id)}>
                        <Button type="link" danger size="small" icon={<DeleteOutlined />} />
                    </Popconfirm>
                </Space>
            ),
        },
    ];

    return (
        <div>
            {/* === SECTION: IMPORT TỪ EXCEL === */}
            <div style={{ 
                padding: 16, marginBottom: 16, 
                background: '#f6ffed', border: '1px solid #b7eb8f', borderRadius: 8
            }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: 12, flexWrap: 'wrap' }}>
                    <FileExcelOutlined style={{ fontSize: 24, color: '#52c41a' }} />
                    <div>
                        <strong>Import Danh Mục từ File Excel</strong>
                        <div style={{ color: '#888', fontSize: 12 }}>
                            Chọn file <code>.xlsx</code> có cột A: STT (La Mã / Số / Số thập phân / trống) và cột B: Nội dung
                        </div>
                    </div>
                    <Upload
                        accept=".xlsx"
                        showUploadList={false}
                        beforeUpload={(file) => {
                            handleImportExcel({ file });
                            return false; // Ngăn Antd tự upload
                        }}
                    >
                        <Button 
                            type="primary" 
                            icon={<UploadOutlined />} 
                            loading={importing}
                            style={{ background: '#52c41a', borderColor: '#52c41a' }}
                        >
                            {importing ? 'Đang import...' : 'Chọn file Excel để Import'}
                        </Button>
                    </Upload>
                    <Button icon={<ReloadOutlined />} onClick={fetchTree}>
                        Làm mới
                    </Button>
                </div>

                {/* Hiển thị kết quả import */}
                {importResult && (
                    <Alert
                        style={{ marginTop: 12 }}
                        type={importResult.type}
                        title={importResult.message}
                        showIcon
                        closable
                        onClose={() => setImportResult(null)}
                    />
                )}
            </div>

            <Divider titlePlacement="left">
                Danh sách Hạng mục ({treeData.length} mục)
            </Divider>

            {/* === SECTION: THÊM THỦ CÔNG === */}
            <div style={{ marginBottom: 12 }}>
                <Button type="default" icon={<PlusOutlined />} onClick={() => {
                    setParentId(null);
                    form.setFieldsValue({ isLeaf: false });
                    setIsModalOpen(true);
                }}>
                    Tạo Nốt Gốc thủ công
                </Button>
            </div>
            
            <Table 
                columns={columns} 
                dataSource={treeData} 
                rowKey="id" 
                pagination={{ pageSize: 30, showTotal: (total) => `Tổng ${total} hạng mục` }}
                bordered 
                size="small"
            />

            <Modal 
                title={parentId ? "Thêm Nốt Con" : "Thêm Mục Gốc"} 
                open={isModalOpen} 
                onOk={form.submit} 
                onCancel={() => setIsModalOpen(false)}
            >
                <Form form={form} layout="vertical" onFinish={handleAddSubmit}>
                    <Form.Item name="title" label="Tên hạng mục" rules={[{ required: true }]}>
                        <Input />
                    </Form.Item>
                    <Form.Item name="code" label="Mã (Dùng khi map Excel)">
                        <Input />
                    </Form.Item>
                    <Form.Item name="isLeaf" label="Là nốt nhập liệu (Leaf)" valuePropName="checked">
                        <Switch />
                    </Form.Item>
                </Form>
            </Modal>
        </div>
    );
};

export default CategoryManager;
