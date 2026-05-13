import React, { useState } from 'react';
import { Card, Form, Input, Button, message, Typography } from 'antd';
import { LockOutlined, UserOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import axiosClient from '../api/axiosClient';
import { useAuthStore } from '../store/useAuthStore';

const { Title } = Typography;

const LoginPage = () => {
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();
    const setCredentials = useAuthStore(state => state.setCredentials);

    const onFinish = async (values) => {
        try {
            setLoading(true);
            // Gọi API Đăng nhập
            const response = await axiosClient.post('/auth/login', {
                username: values.username,
                password: values.password
            });

            // Nếu trả về Token, lưu vào Store
            if (response && response.token) {
                setCredentials(response.token);
                message.success('Đăng nhập thành công!');
                navigate('/', { replace: true });
            }
        } catch (error) {
            message.error('Sai tên đăng nhập hoặc mật khẩu.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh', background: '#f0f2f5' }}>
            <Card style={{ width: 400, boxShadow: '0 4px 8px rgba(0,0,0,0.1)' }}>
                <div style={{ textAlign: 'center', marginBottom: 24 }}>
                    <Title level={3}>Đăng nhập MEMS V2.0</Title>
                    <p style={{ color: '#8c8c8c' }}>Hệ thống quản lý báo cáo tập trung</p>
                </div>
                <Form
                    name="login_form"
                    layout="vertical"
                    onFinish={onFinish}
                >
                    <Form.Item
                        name="username"
                        rules={[{ required: true, message: 'Vui lòng nhập tên đăng nhập!' }]}
                    >
                        <Input prefix={<UserOutlined />} placeholder="Tên đăng nhập" size="large" />
                    </Form.Item>

                    <Form.Item
                        name="password"
                        rules={[{ required: true, message: 'Vui lòng nhập mật khẩu!' }]}
                    >
                        <Input.Password prefix={<LockOutlined />} placeholder="Mật khẩu" size="large" />
                    </Form.Item>

                    <Form.Item>
                        <Button type="primary" htmlType="submit" size="large" block loading={loading}>
                            Đăng nhập
                        </Button>
                    </Form.Item>
                </Form>
            </Card>
        </div>
    );
};

export default LoginPage;
