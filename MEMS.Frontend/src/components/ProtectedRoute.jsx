import React from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import { useAuthStore } from '../store/useAuthStore';

const ProtectedRoute = ({ allowedRoles }) => {
    const { isAuthenticated, user } = useAuthStore();

    // Chưa đăng nhập -> Điều hướng về Đăng nhập (Dùng replace để không lưu lịch sử vào History)
    if (!isAuthenticated) {
        return <Navigate to="/login" replace />;
    }

    // Kiểm tra Role nếu Route có yêu cầu
    if (allowedRoles && allowedRoles.length > 0) {
        if (!user || !allowedRoles.includes(user.role)) {
            return <Navigate to="/403" replace />;
        }
    }

    // Hợp lệ, cho phép render Route con
    return <Outlet />;
};

export default ProtectedRoute;
