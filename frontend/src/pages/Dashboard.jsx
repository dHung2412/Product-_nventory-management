import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import LoadingSpinner from '../components/LoadingSpinner';
import productApi from '../api/productApi';
import warehouseApi from '../api/warehouseApi';
import stockApi from '../api/stockApi';
import userApi from '../api/userApi';
import { ImportStockModal, ExportStockModal } from './StockPage';


const Dashboard = () => {
  const { user } = useAuth();
  const [stats, setStats] = useState({});
  const [lowStockItems, setLowStockItems] = useState([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    loadDashboardData();
  }, []);

    // 👇 thêm state modal
  const [showImportModal, setShowImportModal] = useState(false);
  const [showExportModal, setShowExportModal] = useState(false);
  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      setIsLoading(true);
      
      // Load statistics based on user role
      const promises = [
        productApi.getStatistics(),
        warehouseApi.getStatistics(),
        stockApi.getStatistics(),
        stockApi.getLowStockItems(),
      ];

      // Add user statistics for Admin/Manager
      if (user?.roleName === 'Admin' || user?.roleName === 'Manager') {
        promises.push(userApi.getTotalUsersCount());
        promises.push(userApi.getActiveUsersCount());
      }

      const results = await Promise.all(promises);
      
      const newStats = {
        products: results[0],
        warehouses: results[1],
        stock: results[2],
      };

      if (user?.roleName === 'Admin' || user?.roleName === 'Manager') {
        newStats.totalUsers = results[4];
        newStats.activeUsers = results[5];
      }

      setStats(newStats);
      setLowStockItems(results[3]);
    } catch (error) {
      console.error('Error loading dashboard data:', error);
    } finally {
      setIsLoading(false);
    }
  };

  if (isLoading) {
    return <LoadingSpinner size="large" text="Đang tải dữ liệu dashboard..." />;
  }

  const StatCard = ({ title, value, icon, color, link }) => (
    <div className={`bg-white rounded-lg shadow-md p-6 border-l-4 ${color}`}>
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm font-medium text-gray-600">{title}</p>
          <p className="text-2xl font-bold text-gray-900">{value}</p>
        </div>
        <div className="text-3xl">{icon}</div>
      </div>
      {link && (
        <div className="mt-4">
          <Link to={link} className="text-blue-600 hover:text-blue-800 text-sm font-medium">
            Xem chi tiết →
          </Link>
        </div>
      )}
    </div>
  );

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto py-6 px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Dashboard</h1>
          <p className="mt-2 text-gray-600">
            Chào mừng trở lại, {user?.username}! 
          </p>
        </div>

        {/* Statistics Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <StatCard
            title="Tổng sản phẩm"
            value={stats.products?.totalProducts || 0}
            icon="📦"
            color="border-blue-500"
            link="/products"
          />
          
          <StatCard
            title="Tổng kho hàng"
            value={stats.warehouses?.totalWarehouses || 0}
            icon="🏢"
            color="border-green-500"
            link="/warehouses"
          />
          
          <StatCard
            title="Mặt hàng tồn kho"
            value={stats.stock?.totalStockItems || 0}
            icon="📋"
            color="border-yellow-500"
            link="/stock"
          />
          
          <StatCard
            title="Hàng sắp hết"
            value={stats.stock?.lowStockItemsCount || 0}
            icon="⚠️"
            color="border-red-500"
            link="/stock?filter=low-stock"
          />
        </div>

        {/* Admin/Manager specific stats */}
        {(user?.roleName === 'Admin' || user?.roleName === 'Manager') && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
            <StatCard
              title="Tổng người dùng"
              value={stats.totalUsers || 0}
              icon="👥"
              color="border-purple-500"
              link="/users"
            />
            
            <StatCard
              title="Người dùng hoạt động"
              value={stats.activeUsers || 0}
              icon="✅"
              color="border-indigo-500"
              link="/users?filter=active"
            />
          </div>
        )}

        {/* Quick Actions */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
          {/* Low Stock Items */}
          <div className="bg-white rounded-lg shadow-md p-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold text-gray-900">Hàng sắp hết</h2>
              <Link 
                to="/stock?filter=low-stock" 
                className="text-blue-600 hover:text-blue-800 text-sm font-medium"
              >
                Xem tất cả
              </Link>
            </div>
            
            {lowStockItems.length > 0 ? (
              <div className="space-y-3">
                {lowStockItems.slice(0, 5).map((item) => (
                  <div key={item.id} className="flex items-center justify-between p-3 bg-red-50 rounded-md">
                    <div>
                      <p className="font-medium text-gray-900">{item.productName}</p>
                      <p className="text-sm text-gray-600">{item.warehouseName}</p>
                    </div>
                    <div className="text-right">
                      <p className="text-sm font-medium text-red-600">
                        {item.quantity} / {item.minQuantity}
                      </p>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-gray-500 text-center py-4">Không có hàng sắp hết</p>
            )}
          </div>

          {/* Quick Actions */}
          <div className="bg-white rounded-lg shadow-md p-6">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">Thao tác nhanh</h2>
            <div className="grid grid-cols-2 gap-4">
              <Link
                to="/stock/import"
                className="flex flex-col items-center p-4 bg-green-50 rounded-lg hover:bg-green-100 transition-colors"
              >
                <span className="text-2xl mb-2">📥</span>
                <span className="text-sm font-medium text-green-800">Nhập kho</span>
              </Link>
              
              <Link
                to="/stock/export"
                className="flex flex-col items-center p-4 bg-blue-50 rounded-lg hover:bg-blue-100 transition-colors"
              >
                <span className="text-2xl mb-2">📤</span>
                <span className="text-sm font-medium text-blue-800">Xuất kho</span>
              </Link>
              
              {(user?.roleName === 'Admin' || user?.roleName === 'Manager') && (
                <>
                  <Link
                    to="/products/create"
                    className="flex flex-col items-center p-4 bg-purple-50 rounded-lg hover:bg-purple-100 transition-colors"
                  >
                    <span className="text-2xl mb-2">➕</span>
                    <span className="text-sm font-medium text-purple-800">Thêm sản phẩm</span>
                  </Link>
                  
                  <Link
                    to="/warehouses/create"
                    className="flex flex-col items-center p-4 bg-yellow-50 rounded-lg hover:bg-yellow-100 transition-colors"
                  >
                    <span className="text-2xl mb-2">🏪</span>
                    <span className="text-sm font-medium text-yellow-800">Thêm kho</span>
                  </Link>
                </>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;