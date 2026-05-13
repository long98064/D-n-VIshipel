import React, { useState, useEffect } from 'react';
import { Table, Button, Space, Modal, Form, Input, Select, message, Popconfirm } from 'antd';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import axiosClient from '../../api/axiosClient';

const UserManager = () => {
    const [users, setUsers] = useState([]);
    const [roles, setRoles] = useState([]);
    const [branches, setBranches] = useState([]);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [form] = Form.useForm();

    const fetchUsers = async () => {
        try {
            const res = await axiosClient.get('/admin/users');
            setUsers(res);
        } catch (error) { message.error("Lỗi tải users"); }
    };

    const fetchRoles = async () => {
        try {
            const res = await axiosClient.get('/admin/roles');
            setRoles(res);
        } catch (error) { message.error("Lỗi tải roles"); }
    };

    const fetchBranches = async () => {
        try {
            const res = await axiosClient.get('/admin/branches');
            setBranches(res);
        } catch (error) { message.error("Lỗi tải branches"); }
    };

    useEffect(() => {
        fetchUsers();
        fetchRoles();
        fetchBranches();
    }, []);

    const handleDelete = async (id) => {
        try {
            await axiosClient.delete(`/admin/users/${id}`);
            message.success("Đã xóa User");
            fetchUsers();
        } catch (error) { message.error("Lỗi khi xóa"); }
    };

    const handleAddSubmit = async (values) => {
        try {
            await axiosClient.post('/admin/users', values);
            message.success("Thêm User thành công");
            setIsModalOpen(false);
            form.resetFields();
            fetchUsers();
        } catch (error) { message.error("Lỗi khi thêm User"); }
    };

    const columns = [
        { title: 'Tên Đăng Nhập', dataIndex: 'username', key: 'username' },
        { title: 'Phân Quyền', dataIndex: 'roleName', key: 'roleName' },
        { 
            title: 'Chi Nhánh', 
            dataIndex: 'branchId', 
            render: (val) => branches.find(b => b.id === val)?.name || 'N/A'
        },
        {
            title: 'Hành động',
            key: 'action',
            render: (_, record) => (
                <Space size="middle">
                    <Popconfirm title="Chắc chắn xóa?" onConfirm={() => handleDelete(record.id)}>
                        <Button type="link" danger icon={<DeleteOutlined />} />
                    </Popconfirm>
                </Space>
            ),
        },
    ];

    return (
        <div>
            <div style={{ marginBottom: 16 }}>
                <Button type="primary" icon={<PlusOutlined />} onClick={() => setIsModalOpen(true)}>
                    Tạo Tài Khoản
                </Button>
            </div>
            <Table columns={columns} dataSource={users} rowKey="id" bordered />

            <Modal title="Tạo Tài Khoản" open={isModalOpen} onOk={form.submit} onCancel={() => setIsModalOpen(false)}>
                <Form form={form} layout="vertical" onFinish={handleAddSubmit}>
                    <Form.Item name="username" label="Tên đăng nhập" rules={[{ required: true }]}>
                        <Input />
                    </Form.Item>
                    <Form.Item name="password" label="Mật khẩu" rules={[{ required: true }]}>
                        <Input.Password />
                    </Form.Item>
                    <Form.Item name="roleId" label="Phân quyền (Role)" rules={[{ required: true }]}>
                        <Select>
                            {roles.map(r => <Select.Option key={r.id} value={r.id}>{r.name}</Select.Option>)}
                        </Select>
                    </Form.Item>
                    <Form.Item name="branchId" label="Thuộc Chi Nhánh" rules={[{ required: true }]}>
                        <Select>
                            {branches.map(b => <Select.Option key={b.id} value={b.id}>{b.name}</Select.Option>)}
                        </Select>
                    </Form.Item>
                </Form>
            </Modal>
        </div>
    );
};

export default UserManager;
