import React from 'react';
import { RouterProvider, createBrowserRouter, Outlet } from 'react-router-dom';
import { Layout } from 'antd';
import LoginPage from './pages/LoginPage';
import Dashboard from './pages/Dashboard';
import ReportGrid from './components/ReportGrid';
import Sidebar from './components/Sidebar';
import ProtectedRoute from './components/ProtectedRoute';
import AdminPage from './pages/AdminPage';

const { Header, Content } = Layout;

// Layout chung cho vùng Private (Có Sidebar)
const MainLayout = () => {
    return (
        <Layout style={{ minHeight: '100vh' }}>
            <Sidebar />
            <Layout>
                <Header style={{ background: '#fff', padding: 0, textAlign: 'center', boxShadow: '0 1px 4px rgba(0,0,0,0.1)', zIndex: 1 }}>
                    <h2>MEMS V2.0 Hệ Thống Quản Lý Báo Cáo</h2>
                </Header>
                <Content style={{ margin: '16px' }}>
                    <div style={{ padding: 24, minHeight: 360, background: '#fff', borderRadius: '8px' }}>
                        {/* Khu vực render nội dung của Route con */}
                        <Outlet /> 
                    </div>
                </Content>
            </Layout>
        </Layout>
    );
};

// Cấu hình Router v6 chống Vòng Lặp Redirect
const router = createBrowserRouter([
    {
        path: '/login',
        element: <LoginPage /> // Route Công khai (Public)
    },
    {
        path: '/403',
        element: (
            <div style={{ textAlign: 'center', marginTop: 100 }}>
                <h2>403 Forbidden</h2>
                <p>Bạn không có quyền truy cập trang này.</p>
                <a href="/">Quay về trang chủ</a>
            </div>
        )
    },
    {
        path: '/',
        // Bọc Auth Guard toàn bộ các Route bên trong
        element: <ProtectedRoute allowedRoles={['SuperAdmin', 'Admin', 'Manager', 'User']} />,
        children: [
            {
                element: <MainLayout />, // Render Sidebar + Khung nội dung
                children: [
                    { path: '/', element: <Dashboard /> },
                    { path: '/reports', element: <ReportGrid /> },
                    { 
                        path: '/admin', 
                        // Bọc bảo mật 2 lớp: Chỉ Admin mới vào được /admin
                        element: <ProtectedRoute allowedRoles={['SuperAdmin', 'Admin']} />,
                        children: [
                            { path: '', element: <AdminPage /> }
                        ]
                    }
                ]
            }
        ]
    }
]);

function App() {
  return <RouterProvider router={router} />;
}

export default App;
