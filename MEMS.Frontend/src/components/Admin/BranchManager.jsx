import React, { useState, useEffect } from 'react';
import { Table, Button, Space, Modal, Form, Input, message, Popconfirm } from 'antd';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import axiosClient from '../../api/axiosClient';

const BranchManager = () => {
    const [branches, setBranches] = useState([]);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [form] = Form.useForm();

    const fetchBranches = async () => {
        try {
            const res = await axiosClient.get('/admin/branches');
            setBranches(res);
        } catch (error) { message.error("Lỗi tải branches"); }
    };

    useEffect(() => { fetchBranches(); }, []);

    const handleDelete = async (id) => {
        try {
            await axiosClient.delete(`/admin/branches/${id}`);
            message.success("Đã xóa Chi nhánh");
            fetchBranches();
        } catch (error) { message.error("Lỗi khi xóa"); }
    };

    const handleAddSubmit = async (values) => {
        try {
            await axiosClient.post('/admin/branches', values);
            message.success("Thêm Chi nhánh thành công");
            setIsModalOpen(false);
            form.resetFields();
            fetchBranches();
        } catch (error) { message.error("Lỗi khi thêm Chi nhánh"); }
    };

    const columns = [
        { title: 'Tên Chi Nhánh', dataIndex: 'name', key: 'name' },
        { title: 'Địa chỉ', dataIndex: 'address', key: 'address' },
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
                    Tạo Chi Nhánh Mới
                </Button>
            </div>
            <Table columns={columns} dataSource={branches} rowKey="id" bordered />

            <Modal title="Tạo Chi Nhánh" open={isModalOpen} onOk={form.submit} onCancel={() => setIsModalOpen(false)}>
                <Form form={form} layout="vertical" onFinish={handleAddSubmit}>
                    <Form.Item name="name" label="Tên Chi nhánh" rules={[{ required: true }]}>
                        <Input />
                    </Form.Item>
                    <Form.Item name="address" label="Địa chỉ" rules={[{ required: true }]}>
                        <Input />
                    </Form.Item>
                </Form>
            </Modal>
        </div>
    );
};

export default BranchManager;
