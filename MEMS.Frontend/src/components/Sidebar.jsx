import React from 'react';
import { Menu, Layout, Button } from 'antd';
import { DashboardOutlined, FileExcelOutlined, SettingOutlined, LogoutOutlined } from '@ant-design/icons';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuthStore } from '../store/useAuthStore';

const { Sider } = Layout;

const Sidebar = () => {
    const navigate = useNavigate();
    const location = useLocation();
    const { user, logout } = useAuthStore();

    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    // Menu cơ bản
    const menuItems = [
        { key: '/', icon: <DashboardOutlined />, label: 'Dashboard' },
        { key: '/reports', icon: <FileExcelOutlined />, label: 'Bảng Nhập Báo Cáo' },
    ];

    // Menu ẩn chỉ dành cho Admin
    if (user?.role === 'SuperAdmin' || user?.role === 'Admin') {
        menuItems.push({
            key: '/admin',
            icon: <SettingOutlined />,
            label: 'Quản trị hệ thống',
        });
    }

    return (
        <Sider theme="dark" width={250} style={{ display: 'flex', flexDirection: 'column', height: '100vh' }}>
            <div style={{ padding: '20px', color: '#fff', fontSize: '18px', fontWeight: 'bold', textAlign: 'center', background: 'rgba(255,255,255,0.1)' }}>
                MEMS V2.0
            </div>
            
            <div style={{ flex: 1, overflowY: 'auto' }}>
                <Menu 
                    theme="dark" 
                    mode="inline" 
                    selectedKeys={[location.pathname]}
                    onClick={(e) => navigate(e.key)}
                    items={menuItems}
                />
            </div>

            {/* Logout Button nằm dưới cùng */}
            <div style={{ padding: '16px' }}>
                <Button 
                    type="primary" 
                    danger 
                    icon={<LogoutOutlined />} 
                    block 
                    onClick={handleLogout}
                >
                    Đăng xuất
                </Button>
            </div>
        </Sider>
    );
};

export default Sidebar;
