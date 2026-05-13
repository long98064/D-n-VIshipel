import React, { useState, useEffect } from 'react';
import { Table, message, Tag } from 'antd';
import axiosClient from '../../api/axiosClient';

const AuditLogManager = () => {
    const [data, setData] = useState([]);
    const [loading, setLoading] = useState(false);
    const [pagination, setPagination] = useState({ current: 1, pageSize: 20, total: 0 });

    const fetchLogs = async (page = 1, pageSize = 20) => {
        try {
            setLoading(true);
            const res = await axiosClient.get(`/admin/audit-logs?pageIndex=${page}&pageSize=${pageSize}`);
            setData(res.data);
            setPagination({
                current: res.currentPage,
                pageSize: pageSize,
                total: res.totalRecords
            });
        } catch (error) {
            message.error("Lỗi khi tải Lịch sử truy cập");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => { fetchLogs(1, 20); }, []);

    // Bắt sự kiện chuyển trang của Antd Table để thực hiện Server-Side Pagination
    const handleTableChange = (newPagination) => {
        fetchLogs(newPagination.current, newPagination.pageSize);
    };

    const columns = [
        { title: 'Thời gian', dataIndex: 'createdAt', render: (val) => new Date(val).toLocaleString('vi-VN'), width: '15%' },
        { title: 'Đối tượng', dataIndex: 'entityName', width: '15%', render: (val) => <Tag color="blue">{val}</Tag> },
        { title: 'Hành động', dataIndex: 'action', width: '15%', render: (val) => <Tag color="orange">{val}</Tag> },
        { title: 'Nội dung chi tiết', dataIndex: 'details', width: '40%' },
        { title: 'Người thao tác', dataIndex: 'createdBy', width: '15%' },
    ];

    return (
        <Table 
            columns={columns} 
            dataSource={data} 
            rowKey="id" 
            loading={loading}
            pagination={pagination}
            onChange={handleTableChange}
            bordered
            size="small"
        />
    );
};

export default AuditLogManager;
