import React, { useState, useEffect } from 'react';
import { 
  Layout, Row, Col, Card, DatePicker, Alert, Statistic, 
  Progress, List, Tag, Table, Button, Spin, Typography, Space 
} from 'antd';
import { 
  FileExcelOutlined, FilePdfOutlined, ArrowUpOutlined, ArrowDownOutlined 
} from '@ant-design/icons';
import { 
  ResponsiveContainer, ComposedChart, Line, Bar, XAxis, YAxis, 
  CartesianGrid, Tooltip as RechartsTooltip, Legend 
} from 'recharts';
import dayjs from 'dayjs';
import { useAuthStore } from '../store/useAuthStore';
import axiosClient from '../api/axiosClient';

const { Title, Text } = Typography;
const { Content } = Layout;

// Hàm format tiền tệ VNĐ chuẩn
const formatCurrency = (value) => {
  if (!value) return '0 VNĐ';
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
    maximumFractionDigits: 0
  }).format(value);
};

const Dashboard = () => {
  // Lấy thông tin User từ Zustand Store
  const { user } = useAuthStore();

  // State quản lý
  const [selectedYear, setSelectedYear] = useState(dayjs());
  const [loading, setLoading] = useState(true);
  
  // State dữ liệu
  const [urgentTasks, setUrgentTasks] = useState([]);
  const [kpiSummary, setKpiSummary] = useState({});
  const [chartData, setChartData] = useState([]);
  const [insights, setInsights] = useState([]);
  const [historyParams, setHistoryParams] = useState({ page: 1, limit: 5 });
  const [reportHistory, setReportHistory] = useState({ data: [], total: 0 });

  // GỌI TẤT CẢ API
  const fetchDashboardData = async () => {
    setLoading(true);
    const year = selectedYear.year();
    
    try {
      const [urgentRes, kpiRes, chartRes, insightsRes, historyRes] = await Promise.all([
        axiosClient.get('/api/v2/dashboard/urgent-tasks'),
        axiosClient.get(`/api/v2/dashboard/kpi-summary?year=${year}`),
        axiosClient.get(`/api/v2/dashboard/chart-data?year=${year}`),
        axiosClient.get(`/api/v2/dashboard/insights?year=${year}`),
        axiosClient.get(`/api/v2/dashboard/history?page=${historyParams.page}&limit=${historyParams.limit}`)
      ]);

      setUrgentTasks(urgentRes.data.tasks || []);
      setKpiSummary(kpiRes.data || {});
      setChartData(chartRes.data.data || []);
      setInsights(insightsRes.data.insights || []);
      setReportHistory({
        data: historyRes.data.items || [],
        total: historyRes.data.totalCount || 0
      });
    } catch (error) {
      console.error("Lỗi khi tải dữ liệu Dashboard:", error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDashboardData();
  }, [selectedYear, historyParams]);

  // Cấu hình cột cho Bảng Lịch sử Báo cáo (Giai đoạn 4)
  const historyColumns = [
    { title: 'Kỳ báo cáo', dataIndex: 'period', key: 'period' },
    { 
      title: 'Doanh thu', 
      dataIndex: 'totalRevenue', 
      key: 'totalRevenue',
      render: (val) => <Text strong>{formatCurrency(val)}</Text>
    },
    { title: 'Người nộp', dataIndex: 'submittedBy', key: 'submittedBy' },
    { 
      title: 'Trạng thái', 
      dataIndex: 'status', 
      key: 'status',
      render: (status) => {
        let color = status === 'APPROVED' ? 'success' : status === 'REJECTED' ? 'error' : 'warning';
        return <Tag color={color}>{status}</Tag>;
      }
    },
    {
      title: 'Thao tác',
      key: 'action',
      render: (_, record) => (
        <Space size="middle">
          <Button type="text" icon={<FileExcelOutlined style={{ color: '#52c41a' }} />} />
          <Button type="text" icon={<FilePdfOutlined style={{ color: '#ff4d4f' }} />} />
        </Space>
      ),
    },
  ];

  return (
    <Layout style={{ padding: '24px', background: '#f0f2f5', minHeight: '100vh' }}>
      <Content>
        {/* GIAI ĐOẠN 1: HEADER & LỌC TOÀN CỤC */}
        <Row justify="space-between" align="middle" style={{ marginBottom: 24 }}>
          <Col>
            <Title level={3} style={{ margin: 0 }}>Tổng quan Chi nhánh</Title>
            <Text type="secondary">
              Xin chào {user?.fullName || 'Người dùng'} - {user?.role} | Chi nhánh: {user?.branchName}
            </Text>
          </Col>
          <Col>
            <DatePicker 
              picker="year" 
              value={selectedYear} 
              onChange={(date) => date && setSelectedYear(date)}
              allowClear={false}
            />
          </Col>
        </Row>

        <Spin spinning={loading} tip="Đang tải dữ liệu...">
          {/* GIAI ĐOẠN 2: KHU VỰC 1 (Cảnh báo khẩn cấp) */}
          {urgentTasks.length > 0 && (
            <Row style={{ marginBottom: 24 }}>
              <Col span={24}>
                {urgentTasks.map((task) => (
                  <Alert
                    key={task.reportId}
                    message={task.message}
                    type={task.type}
                    showIcon
                    action={<Button size="small" danger>Sửa báo cáo ngay</Button>}
                    style={{ marginBottom: 8 }}
                  />
                ))}
              </Col>
            </Row>
          )}

          {/* GIAI ĐOẠN 2: KHU VỰC 2 (KPI Tổng quan) */}
          <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
            <Col xs={24} sm={8}>
              <Card bordered={false} style={{ borderRadius: 8, boxShadow: '0 1px 2px rgba(0,0,0,0.05)' }}>
                <Statistic
                  title="Doanh thu lũy kế"
                  value={kpiSummary.totalRevenue || 0}
                  formatter={(value) => formatCurrency(value)}
                  valueStyle={{ color: kpiSummary.isGrowth ? '#3f8600' : '#cf1322' }}
                  prefix={kpiSummary.isGrowth ? <ArrowUpOutlined /> : <ArrowDownOutlined />}
                />
              </Card>
            </Col>
            <Col xs={24} sm={8}>
              <Card bordered={false} style={{ borderRadius: 8, boxShadow: '0 1px 2px rgba(0,0,0,0.05)' }}>
                <Statistic title="Mục tiêu năm" value={formatCurrency(kpiSummary.targetAmount || 0)} />
                <Progress 
                  percent={kpiSummary.completionRate || 0} 
                  status={kpiSummary.completionRate >= 100 ? "success" : "active"} 
                  strokeColor={{ '0%': '#108ee9', '100%': '#87d068' }}
                />
              </Card>
            </Col>
            <Col xs={24} sm={8}>
              <Card bordered={false} style={{ borderRadius: 8, boxShadow: '0 1px 2px rgba(0,0,0,0.05)' }}>
                <Statistic title="Trạng thái tăng trưởng" value="Tích cực" />
                <Text type="secondary">Hoạt động kinh doanh đang đi đúng hướng Kế hoạch đề ra.</Text>
              </Card>
            </Col>
          </Row>

          {/* GIAI ĐOẠN 3: KHU VỰC 3 (Biểu đồ & Khuyến nghị) */}
          <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
            {/* Biểu đồ (16 cột) */}
            <Col xs={24} lg={16}>
              <Card title="Thực tế vs Kế hoạch" bordered={false} style={{ borderRadius: 8, height: '100%' }}>
                <div style={{ width: '100%', height: 300 }}>
                  <ResponsiveContainer>
                    <ComposedChart data={chartData}>
                      <CartesianGrid strokeDasharray="3 3" vertical={false} />
                      <XAxis dataKey="month" />
                      <YAxis tickFormatter={(value) => `${value / 1000000}M`} />
                      <RechartsTooltip formatter={(value) => formatCurrency(value)} />
                      <Legend />
                      <Bar dataKey="actualAmount" name="Thực tế" fill="#1890ff" radius={[4, 4, 0, 0]} />
                      <Line type="monotone" dataKey="targetAmount" name="Kế hoạch" stroke="#faad14" strokeWidth={2} dot={{ r: 4 }} />
                    </ComposedChart>
                  </ResponsiveContainer>
                </div>
              </Card>
            </Col>
            
            {/* Khuyến nghị (8 cột) */}
            <Col xs={24} lg={8}>
              <Card title="Khuyến nghị hệ thống" bordered={false} style={{ borderRadius: 8, height: '100%' }}>
                <List
                  itemLayout="horizontal"
                  dataSource={insights}
                  renderItem={(item) => (
                    <List.Item>
                      <List.Item.Meta
                        title={<Tag color={item.color}>{item.color === 'error' ? 'Cảnh báo' : 'Tốt'}</Tag>}
                        description={item.message}
                      />
                    </List.Item>
                  )}
                />
              </Card>
            </Col>
          </Row>

          {/* GIAI ĐOẠN 4: KHU VỰC 4 (Lịch sử Báo cáo) */}
          <Row>
            <Col span={24}>
              <Card title="Lịch sử báo cáo định kỳ" bordered={false} style={{ borderRadius: 8 }}>
                <Table 
                  columns={historyColumns} 
                  dataSource={reportHistory.data} 
                  rowKey="id"
                  pagination={{
                    current: historyParams.page,
                    pageSize: historyParams.limit,
                    total: reportHistory.total,
                    onChange: (page, pageSize) => setHistoryParams({ page, limit: pageSize })
                  }}
                />
              </Card>
            </Col>
          </Row>
        </Spin>
      </Content>
    </Layout>
  );
};

export default Dashboard;