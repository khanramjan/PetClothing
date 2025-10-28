import { useEffect, useState, useCallback } from 'react';
import { FaShoppingCart, FaBox, FaUsers, FaDollarSign, FaArrowUp, FaArrowDown, FaSync } from 'react-icons/fa';
import { LineChart, Line, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import { useNavigate } from 'react-router-dom';
import api from '@/lib/api';
import { toast } from 'react-toastify';

interface DashboardStats {
  totalOrders: number;
  pendingOrders: number;
  totalProducts: number;
  lowStockProducts: number;
  totalCustomers: number;
  newCustomersThisMonth: number;
  totalRevenue: number;
  monthlyRevenue: number;
  ordersByStatus: Array<{ status: string; count: number }>;
  revenueChart: Array<{ month: string; revenue: number }>;
  topProducts: Array<{
    productId: number;
    productName: string;
    imageUrl: string;
    totalSold: number;
    revenue: number;
  }>;
}

export default function Dashboard() {
  const navigate = useNavigate();
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [lastUpdated, setLastUpdated] = useState<Date>(new Date());

  const fetchDashboardStats = useCallback(async (showRefreshIndicator = false) => {
    try {
      if (showRefreshIndicator) {
        setRefreshing(true);
      } else {
        setLoading(true);
      }
      
      const response = await api.get('/admin/dashboard');
      setStats(response.data.data);
      setLastUpdated(new Date());
      
      if (showRefreshIndicator) {
        toast.success('Dashboard refreshed!', { autoClose: 2000 });
      }
    } catch (error) {
      toast.error('Failed to load dashboard stats');
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => {
    fetchDashboardStats();
    
    // Auto-refresh every 30 seconds for real-time updates
    const interval = setInterval(() => {
      fetchDashboardStats(false);
    }, 30000);
    
    return () => clearInterval(interval);
  }, [fetchDashboardStats]);

  const handleRefresh = () => {
    fetchDashboardStats(true);
  };

  if (loading) {
    return (
      <div className="flex flex-col items-center justify-center h-[70vh]">
        <div className="spinner mb-4"></div>
        <p className="text-gray-600">Loading dashboard...</p>
      </div>
    );
  }

  if (!stats) {
    return (
      <div className="p-6 text-center">
        <p className="text-red-600 mb-4">Failed to load dashboard data</p>
        <button onClick={() => fetchDashboardStats()} className="btn btn-primary">
          Retry
        </button>
      </div>
    );
  }

  const StatCard = ({ title, value, icon: Icon, color, change, changeType, onClick }: any) => (
    <div 
      className={`card hover:shadow-xl transition-all duration-300 ${onClick ? 'cursor-pointer' : ''}`}
      onClick={onClick}
    >
      <div className="flex items-center justify-between">
        <div className="flex-1">
          <p className="text-sm font-medium text-gray-600 mb-1">{title}</p>
          <p className="text-3xl font-bold text-gray-900">{value}</p>
          {change && (
            <p className={`text-sm mt-2 flex items-center ${changeType === 'up' ? 'text-green-600' : changeType === 'down' ? 'text-red-600' : 'text-gray-600'}`}>
              {changeType === 'up' && <FaArrowUp className="mr-1" />}
              {changeType === 'down' && <FaArrowDown className="mr-1" />}
              {change}
            </p>
          )}
        </div>
        <div className={`p-4 rounded-full ${color} transform hover:scale-110 transition-transform`}>
          <Icon className="text-white text-2xl" />
        </div>
      </div>
    </div>
  );

  return (
    <div className="space-y-6 animate-fadeIn">
      {/* Header with Refresh */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Dashboard Overview</h1>
          <p className="text-sm text-gray-500 mt-1">
            Last updated: {lastUpdated.toLocaleTimeString()}
          </p>
        </div>
        <button
          onClick={handleRefresh}
          disabled={refreshing}
          className="flex items-center gap-2 px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 transition disabled:opacity-50"
        >
          <FaSync className={refreshing ? 'animate-spin' : ''} />
          {refreshing ? 'Refreshing...' : 'Refresh'}
        </button>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <StatCard
          title="Total Orders"
          value={stats.totalOrders.toLocaleString()}
          icon={FaShoppingCart}
          color="bg-blue-500"
          change={`${stats.pendingOrders} pending`}
          changeType={stats.pendingOrders > 0 ? 'up' : 'neutral'}
          onClick={() => navigate('/admin/orders')}
        />
        <StatCard
          title="Total Products"
          value={stats.totalProducts.toLocaleString()}
          icon={FaBox}
          color="bg-green-500"
          change={stats.lowStockProducts > 0 ? `${stats.lowStockProducts} low stock` : 'All stocked'}
          changeType={stats.lowStockProducts > 0 ? 'down' : 'neutral'}
          onClick={() => navigate('/admin/products')}
        />
        <StatCard
          title="Total Customers"
          value={stats.totalCustomers.toLocaleString()}
          icon={FaUsers}
          color="bg-purple-500"
          change={`+${stats.newCustomersThisMonth} this month`}
          changeType="up"
          onClick={() => navigate('/admin/customers')}
        />
        <StatCard
          title="Total Revenue"
          value={`$${stats.totalRevenue.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`}
          icon={FaDollarSign}
          color="bg-yellow-500"
          change={`$${stats.monthlyRevenue.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })} this month`}
          changeType="up"
        />
      </div>

      {/* Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Revenue Chart */}
        <div className="card">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-xl font-semibold">Revenue Trend</h2>
            <span className="text-xs text-gray-500">Last 6 Months</span>
          </div>
          {stats.revenueChart && stats.revenueChart.length > 0 ? (
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={stats.revenueChart}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis dataKey="month" stroke="#6b7280" />
                <YAxis stroke="#6b7280" />
                <Tooltip 
                  contentStyle={{ backgroundColor: '#fff', border: '1px solid #e5e7eb', borderRadius: '8px' }}
                  formatter={(value: any) => `$${value.toFixed(2)}`}
                />
                <Legend />
                <Line 
                  type="monotone" 
                  dataKey="revenue" 
                  stroke="#8b5cf6" 
                  strokeWidth={3} 
                  name="Revenue ($)"
                  dot={{ fill: '#8b5cf6', r: 5 }}
                  activeDot={{ r: 8 }}
                />
              </LineChart>
            </ResponsiveContainer>
          ) : (
            <div className="h-[300px] flex items-center justify-center text-gray-400">
              No revenue data available
            </div>
          )}
        </div>

        {/* Orders by Status */}
        <div className="card">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-xl font-semibold">Orders by Status</h2>
            <span className="text-xs text-gray-500">Current</span>
          </div>
          {stats.ordersByStatus && stats.ordersByStatus.length > 0 ? (
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={stats.ordersByStatus}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis dataKey="status" stroke="#6b7280" />
                <YAxis stroke="#6b7280" />
                <Tooltip 
                  contentStyle={{ backgroundColor: '#fff', border: '1px solid #e5e7eb', borderRadius: '8px' }}
                />
                <Legend />
                <Bar dataKey="count" fill="#3b82f6" name="Orders" radius={[8, 8, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <div className="h-[300px] flex items-center justify-center text-gray-400">
              No order data available
            </div>
          )}
        </div>
      </div>

      {/* Top Products */}
      <div className="card">
        <h2 className="text-xl font-semibold mb-4">Top 10 Best-Selling Products</h2>
        {stats.topProducts && stats.topProducts.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Rank</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Product</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Units Sold</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Revenue</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {stats.topProducts.map((product, index) => (
                  <tr key={product.productId} className="hover:bg-gray-50 transition">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className={`flex items-center justify-center w-8 h-8 rounded-full font-bold ${
                        index === 0 ? 'bg-yellow-100 text-yellow-600' :
                        index === 1 ? 'bg-gray-100 text-gray-600' :
                        index === 2 ? 'bg-orange-100 text-orange-600' :
                        'bg-primary-100 text-primary-600'
                      }`}>
                        {index + 1}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex items-center">
                        <img
                          src={product.imageUrl || '/placeholder.png'}
                          alt={product.productName}
                          className="h-12 w-12 rounded-lg object-cover mr-3 shadow-sm"
                          onError={(e) => {
                            (e.target as HTMLImageElement).src = '/placeholder.png';
                          }}
                        />
                        <div className="text-sm font-medium text-gray-900">{product.productName}</div>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm text-gray-900 font-semibold">{product.totalSold.toLocaleString()} units</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-bold text-green-600">
                        ${product.revenue.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="py-12 text-center text-gray-400">
            No product sales data available yet
          </div>
        )}
      </div>

      {/* Quick Actions */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div 
          className="card hover:shadow-lg transition-all cursor-pointer transform hover:scale-105"
          onClick={() => navigate('/admin/orders?status=Pending')}
        >
          <div className="flex items-center gap-4">
            <div className="p-3 bg-red-100 rounded-full">
              <FaShoppingCart className="text-red-600 text-xl" />
            </div>
            <div className="flex-1">
              <p className="text-sm text-gray-600">Pending Orders</p>
              <p className="text-2xl font-bold">{stats.pendingOrders}</p>
              <p className="text-sm text-blue-600 mt-1 hover:underline">View all pending orders →</p>
            </div>
          </div>
        </div>

        <div 
          className="card hover:shadow-lg transition-all cursor-pointer transform hover:scale-105"
          onClick={() => navigate('/admin/products?lowStock=true')}
        >
          <div className="flex items-center gap-4">
            <div className="p-3 bg-orange-100 rounded-full">
              <FaBox className="text-orange-600 text-xl" />
            </div>
            <div className="flex-1">
              <p className="text-sm text-gray-600">Low Stock Products</p>
              <p className="text-2xl font-bold">{stats.lowStockProducts}</p>
              <p className="text-sm text-blue-600 mt-1 hover:underline">View low stock items →</p>
            </div>
          </div>
        </div>

        <div 
          className="card hover:shadow-lg transition-all cursor-pointer transform hover:scale-105"
          onClick={() => navigate('/admin/customers?filter=new')}
        >
          <div className="flex items-center gap-4">
            <div className="p-3 bg-green-100 rounded-full">
              <FaUsers className="text-green-600 text-xl" />
            </div>
            <div className="flex-1">
              <p className="text-sm text-gray-600">New Customers</p>
              <p className="text-2xl font-bold">{stats.newCustomersThisMonth}</p>
              <p className="text-sm text-blue-600 mt-1 hover:underline">View all customers →</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}


