import React, { useState } from 'react';
import { Tabs, Typography, Button, message, Space } from 'antd';
import { DownloadOutlined } from '@ant-design/icons';
import CategoryManager from '../components/Admin/CategoryManager';
import UserManager from '../components/Admin/UserManager';
import BranchManager from '../components/Admin/BranchManager';
import AuditLogManager from '../components/Admin/AuditLogManager';
import axiosClient from '../api/axiosClient';

const { Title } = Typography;

const AdminPage = () => {
    const [isBackingUp, setIsBackingUp] = useState(false);

    const handleBackup = async () => {
        try {
            setIsBackingUp(true);
            message.loading({ content: 'Đang ra lệnh Backup Database. Vui lòng đợi...', key: 'backup' });
            
            // Yêu cầu File Stream từ API Backup
            const response = await axiosClient.post('/admin/backup/trigger', {}, { responseType: 'blob' });
            
            // Xử lý tạo URL tĩnh để trình duyệt tải file
            const url = window.URL.createObjectURL(new Blob([response]));
            const link = document.createElement('a');
            link.href = url;
            const dateStr = new Date().toISOString().slice(0,10).replace(/-/g, '');
            link.setAttribute('download', `MEMS_Backup_${dateStr}.bak`);
            document.body.appendChild(link);
            link.click();
            link.remove();
            
            message.success({ content: 'Tải Backup thành công!', key: 'backup' });
        } catch (error) {
            message.error({ content: 'Lỗi Backup! Đảm bảo SQL Server có quyền ghi vào C:\\Temp', key: 'backup' });
        } finally {
            setIsBackingUp(false);
        }
    };

    const tabItems = [
        { key: '1', label: 'Quản lý Danh mục (Cây)', children: <CategoryManager /> },
        { key: '2', label: 'Quản lý Tài khoản', children: <UserManager /> },
        { key: '3', label: 'Cơ cấu Chi nhánh', children: <BranchManager /> },
        { key: '4', label: 'Lịch sử Hệ thống (Audit Logs)', children: <AuditLogManager /> },
    ];

    return (
        <div style={{ padding: 20 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
                <Title level={2} style={{ margin: 0 }}>Quản Trị Hệ Thống</Title>
                <Space>
                    <Button 
                        type="primary" 
                        danger 
                        icon={<DownloadOutlined />} 
                        loading={isBackingUp} 
                        onClick={handleBackup}
                    >
                        Sao lưu Hệ thống (Database Backup)
                    </Button>
                </Space>
            </div>
            
            <Tabs defaultActiveKey="1" type="card" items={tabItems} />
        </div>
    );
};

export default AdminPage;
