import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import LoadingSpinner from '../components/LoadingSpinner';
import warehouseApi from '../api/warehouseApi';
import { useNavigate } from 'react-router-dom';


const WarehousesPage = () => {
  const { user } = useAuth();
  const [warehouses, setWarehouses] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [selectedWarehouse, setSelectedWarehouse] = useState(null);

  useEffect(() => {
    loadWarehouses();
  }, []);

  const loadWarehouses = async () => {
    try {
      setIsLoading(true);
      const data = await warehouseApi.getAll();
      console.log("WAREHOUSE API DATA:", data);
      setWarehouses(data);
    } catch (error) {
      console.error('Error loading warehouses:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleDelete = async (id) => {
    try {
      const canDelete = await warehouseApi.canDelete(id);
      if (!canDelete.canDelete) {
        alert('Không thể xóa kho này vì còn chứa hàng hóa');
        return;
      }

      if (window.confirm('Bạn có chắc chắn muốn xóa kho này?')) {
        await warehouseApi.delete(id);
        loadWarehouses();
      }
    } catch (error) {
      console.error('Error deleting warehouse:', error);
      alert('Có lỗi xảy ra khi xóa kho');
    }
  };

  const canManageWarehouses = user?.roleName  === 'Admin' || user?.roleName  === 'Manager';

  if (isLoading && warehouses.length === 0) {
    return <LoadingSpinner size="large" text="Đang tải danh sách kho..." />;
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto py-6 px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="flex justify-between items-center mb-6">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">Quản lý kho hàng</h1>
            <p className="mt-2 text-gray-600">
              Tổng số: {warehouses.length} kho
            </p>
          </div>
          {canManageWarehouses && (
            <button
              onClick={() => setShowCreateModal(true)}
              className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-md font-medium transition-colors"
            >
              ➕ Thêm kho
            </button>
          )}
        </div>

        {/* Warehouses Grid */}
        {isLoading ? (
          <div className="flex justify-center py-8">
            <LoadingSpinner size="medium" text="Đang tải..." />
          </div>
        ) : warehouses.length === 0 ? (
          <div className="text-center py-8">
            <p className="text-gray-500">Chưa có kho nào</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {warehouses.map((warehouse) => (
              <WarehouseCard
                key={warehouse.id}
                warehouse={warehouse}
                canManage={canManageWarehouses}
                onEdit={(warehouse) => {
                  setSelectedWarehouse(warehouse);
                  setShowEditModal(true);
                }}
                onDelete={() => handleDelete(warehouse.id)}
              />
            ))}
          </div>
        )}
      </div>

      {/* Modals */}
      {showCreateModal && (
        <CreateWarehouseModal
          onClose={() => setShowCreateModal(false)}
          onSuccess={() => {
            setShowCreateModal(false);
            loadWarehouses();
          }}
        />
      )}

      {showEditModal && selectedWarehouse && (
        <EditWarehouseModal
          warehouse={selectedWarehouse}
          onClose={() => {
            setShowEditModal(false);
            setSelectedWarehouse(null);
          }}
          onSuccess={() => {
            setShowEditModal(false);
            setSelectedWarehouse(null);
            loadWarehouses();
          }}
        />
      )}
    </div>
  );
};

// Warehouse Card Component
const WarehouseCard = ({ warehouse, canManage, onEdit, onDelete }) => {
  const navigate = useNavigate();
  return (
    <div className="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow">
      <div className="flex items-start justify-between mb-4">
        <div className="flex items-center">
          <span className="text-2xl mr-3">🏢</span>
          <div>
            <h3 className="text-lg font-semibold text-gray-900">{warehouse.name}</h3>
            <p className="text-sm text-gray-500">{warehouse.code}</p>
          </div>
        </div>
        {canManage && (
          <div className="flex space-x-2">
            <button
              onClick={() => onEdit(warehouse)}
              className="text-blue-600 hover:text-blue-800 text-sm"
            >
              ✏️
            </button>
            <button
              onClick={onDelete}
              className="text-red-600 hover:text-red-800 text-sm"
            >
              🗑️
            </button>
          </div>
        )}
      </div>

      {warehouse.description && (
        <p className="text-gray-600 mb-4 text-sm">{warehouse.description}</p>
      )}

      <div className="space-y-2 text-sm">
        {warehouse.address && (
          <div className="flex items-center text-gray-600">
            <span className="mr-2">📍</span>
            <span>{warehouse.address}</span>
          </div>
        )}
        
        {warehouse.phoneNumber && (
          <div className="flex items-center text-gray-600">
            <span className="mr-2">📞</span>
            <span>{warehouse.phoneNumber}</span>
          </div>
        )}
        
        {warehouse.email && (
          <div className="flex items-center text-gray-600">
            <span className="mr-2">📧</span>
            <span>{warehouse.email}</span>
          </div>
        )}
      </div>

      <div className="mt-4 pt-4 border-t border-gray-200">
        <div className="flex items-center justify-between">
          <span className={`px-2 py-1 text-xs rounded-full ${
            warehouse.totalStock > 0
              ? 'bg-green-100 text-green-800' 
              : 'bg-red-100 text-red-800'
          }`}>
            {warehouse.totalStock > 0 ? 'Hoạt động' : 'Ngừng hoạt động'}
          </span>
          <button
            className="text-blue-600 hover:text-blue-800 text-sm font-medium"
            onClick={() => {
              // Navigate to warehouse stock view
              navigate(`/warehouses/${warehouse.id}/stock`);
            }}
          >
            Xem tồn kho →
          </button>
        </div>
      </div>
    </div>
  );
};

// Create Warehouse Modal
const CreateWarehouseModal = ({ onClose, onSuccess }) => {
  const [formData, setFormData] = useState({
    name: '',
    code: '',
    description: '',
    address: '',
    phoneNumber: '',
    email: ''
  });
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      setIsLoading(true);
      setError('');
      
      await warehouseApi.create(formData);
      onSuccess();
    } catch (error) {
      setError(error.message);
    } finally {
      setIsLoading(false);
    }
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  return (
    <div className="fixed inset-0 bg-gray-600 bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-6 w-full max-w-md">
        <h3 className="text-lg font-medium text-gray-900 mb-4">Thêm kho mới</h3>
        
        {error && (
          <div className="mb-4 bg-red-50 border border-red-200 rounded-md p-3">
            <p className="text-red-800 text-sm">{error}</p>
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Tên kho *
              </label>
              <input
                type="text"
                name="name"
                required
                value={formData.name}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Mã kho *
              </label>
              <input
                type="text"
                name="code"
                required
                value={formData.code}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Mô tả
              </label>
              <textarea
                name="description"
                rows="3"
                value={formData.description}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Địa chỉ
              </label>
              <input
                type="text"
                name="address"
                value={formData.address}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Số điện thoại
              </label>
              <input
                type="tel"
                name="phoneNumber"
                value={formData.phoneNumber}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Email
              </label>
              <input
                type="email"
                name="email"
                value={formData.email}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
          </div>

          <div className="flex justify-end space-x-3 mt-6">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50 transition-colors"
            >
              Hủy
            </button>
            <button
              type="submit"
              disabled={isLoading}
              className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors disabled:opacity-50"
            >
              {isLoading ? 'Đang lưu...' : 'Lưu'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

// Edit Warehouse Modal
const EditWarehouseModal = ({ warehouse, onClose, onSuccess }) => {
  const [formData, setFormData] = useState({
    name: warehouse.name || '',
    code: warehouse.code || '',
    description: warehouse.description || '',
    address: warehouse.address || '',
    phoneNumber: warehouse.phoneNumber || '',
    email: warehouse.email || '',
    // isActive: warehouse.isActive
  });
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      setIsLoading(true);
      setError('');
      
      await warehouseApi.update(warehouse.id, formData);
      onSuccess();
    } catch (error) {
      setError(error.message);
    } finally {
      setIsLoading(false);
    }
  };

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  return (
    <div className="fixed inset-0 bg-gray-600 bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-6 w-full max-w-md">
        <h3 className="text-lg font-medium text-gray-900 mb-4">Chỉnh sửa kho</h3>
        
        {error && (
          <div className="mb-4 bg-red-50 border border-red-200 rounded-md p-3">
            <p className="text-red-800 text-sm">{error}</p>
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Tên kho *
              </label>
              <input
                type="text"
                name="name"
                required
                value={formData.name}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Mã kho *
              </label>
              <input
                type="text"
                name="code"
                required
                value={formData.code}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Mô tả
              </label>
              <textarea
                name="description"
                rows="3"
                value={formData.description}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Địa chỉ
              </label>
              <input
                type="text"
                name="address"
                value={formData.address}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Số điện thoại
              </label>
              <input
                type="tel"
                name="phoneNumber"
                value={formData.phoneNumber}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Email
              </label>
              <input
                type="email"
                name="email"
                value={formData.email}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            {/* <div>
              <label className="flex items-center">
                <input
                  type="checkbox"
                  name="isActive"
                  checked={formData.isActive}
                  onChange={handleChange}
                  className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <span className="ml-2 text-sm text-gray-700">Kích hoạt</span>
              </label>
            </div> */}
          </div>

          <div className="flex justify-end space-x-3 mt-6">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50 transition-colors"
            >
              Hủy
            </button>
            <button
              type="submit"
              disabled={isLoading}
              className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors disabled:opacity-50"
            >
              {isLoading ? 'Đang cập nhật...' : 'Cập nhật'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default WarehousesPage;