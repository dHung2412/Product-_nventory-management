import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import LoadingSpinner from '../components/LoadingSpinner';
import stockApi from '../api/stockApi';
import productApi from '../api/productApi';
import warehouseApi from '../api/warehouseApi';
import { formatDateTime, getTransactionTypeDisplayName } from '../utils/helpers';

const StockPage = () => {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState('items');
  const [stockItems, setStockItems] = useState([]);
  const [transactions, setTransactions] = useState([]);
  const [lowStockItems, setLowStockItems] = useState([]);
  const [products, setProducts] = useState([]);
  const [warehouses, setWarehouses] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [selectedProduct, setSelectedProduct] = useState('');
  const [selectedWarehouse, setSelectedWarehouse] = useState('');
  const [showImportModal, setShowImportModal] = useState(false);
  const [showExportModal, setShowExportModal] = useState(false);
  const [showAdjustModal, setShowAdjustModal] = useState(false);

  useEffect(() => {
    loadInitialData();
  }, []);

  useEffect(() => {
    if (activeTab === 'items') {
      loadStockItems();
    } else if (activeTab === 'transactions') {
      loadTransactions();
    } else if (activeTab === 'alerts') {
      loadLowStockItems();
    }
  }, [activeTab, selectedProduct, selectedWarehouse]);

  const loadInitialData = async () => {
    try {
      const [productsData, warehousesData] = await Promise.all([
        productApi.getAll(),
        warehouseApi.getAll()
      ]);
      setProducts(productsData);
      setWarehouses(warehousesData);
    } catch (error) {
      console.error('Error loading initial data:', error);
    }
  };

  const loadStockItems = async () => {
    try {
      setIsLoading(true);
      let data;
      
      if (selectedProduct && selectedWarehouse) {
        // Filter by both product and warehouse
        const productItems = await stockApi.getStockItemsByProduct(selectedProduct);
        data = productItems.filter(item => item.warehouseId === selectedWarehouse);
      } else if (selectedProduct) {
        data = await stockApi.getStockItemsByProduct(selectedProduct);
      } else if (selectedWarehouse) {
        data = await stockApi.getStockItemsByWarehouse(selectedWarehouse);
      } else {
        data = await stockApi.getAllStockItems();
      }
      
      setStockItems(data);
    } catch (error) {
      console.error('Error loading stock items:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const loadTransactions = async () => {
    try {
      setIsLoading(true);
      let data;
      
      if (selectedProduct && selectedWarehouse) {
        // Get transactions for specific product and warehouse
        const productTransactions = await stockApi.getTransactionsByProduct(selectedProduct);
        data = productTransactions.filter(tx => tx.warehouseId === selectedWarehouse);
      } else if (selectedProduct) {
        data = await stockApi.getTransactionsByProduct(selectedProduct);
      } else if (selectedWarehouse) {
        data = await stockApi.getTransactionsByWarehouse(selectedWarehouse);
      } else {
        data = await stockApi.getAllTransactions();
      }
      
      // Sort by date descending
      data.sort((a, b) => new Date(b.transactionDate) - new Date(a.transactionDate));
      setTransactions(data);
    } catch (error) {
      console.error('Error loading transactions:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const loadLowStockItems = async () => {
    try {
      setIsLoading(true);
      const data = await stockApi.getLowStockItems();
      setLowStockItems(data);
    } catch (error) {
      console.error('Error loading low stock items:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const getProductName = (productId) => {
    const product = products.find(p => p.id === productId);
    return product ? product.name : 'N/A';
  };

  const getWarehouseName = (warehouseId) => {
    const warehouse = warehouses.find(w => w.id === warehouseId);
    return warehouse ? warehouse.name : 'N/A';
  };

  const canManageStock = user?.role === 'Admin' || user?.role === 'Manager' || user?.role === 'Employee';

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto py-6 px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="flex justify-between items-center mb-6">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">Quản lý tồn kho</h1>
            <p className="mt-2 text-gray-600">Theo dõi và quản lý hàng tồn kho</p>
          </div>
          {canManageStock && (
            <div className="flex space-x-3">
              <button
                onClick={() => setShowImportModal(true)}
                className="bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded-md font-medium transition-colors"
              >
                📥 Nhập kho
              </button>
              <button
                onClick={() => setShowExportModal(true)}
                className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-md font-medium transition-colors"
              >
                📤 Xuất kho
              </button>
              {(user?.role === 'Admin' || user?.role === 'Manager') && (
                <button
                  onClick={() => setShowAdjustModal(true)}
                  className="bg-yellow-600 hover:bg-yellow-700 text-white px-4 py-2 rounded-md font-medium transition-colors"
                >
                  ⚖️ Điều chỉnh
                </button>
              )}
            </div>
          )}
        </div>

        {/* Tabs */}
        <div className="mb-6">
          <nav className="flex space-x-8">
            {[
              { key: 'items', label: 'Tồn kho', icon: '📦' },
              { key: 'transactions', label: 'Lịch sử giao dịch', icon: '📋' },
              { key: 'alerts', label: 'Cảnh báo tồn kho', icon: '⚠️' },
            ].map((tab) => (
              <button
                key={tab.key}
                onClick={() => setActiveTab(tab.key)}
                className={`flex items-center px-3 py-2 font-medium text-sm rounded-md transition-colors ${
                  activeTab === tab.key
                    ? 'bg-blue-100 text-blue-700'
                    : 'text-gray-600 hover:text-gray-900 hover:bg-gray-100'
                }`}
              >
                <span className="mr-2">{tab.icon}</span>
                {tab.label}
              </button>
            ))}
          </nav>
        </div>

        {/* Filters */}
        {(activeTab === 'items' || activeTab === 'transactions') && (
          <div className="bg-white rounded-lg shadow-md p-6 mb-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Sản phẩm
                </label>
                <select
                  value={selectedProduct}
                  onChange={(e) => setSelectedProduct(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  <option value="">Tất cả sản phẩm</option>
                  {products.map((product) => (
                    <option key={product.id} value={product.id}>
                      {product.name}
                    </option>
                  ))}
                </select>
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Kho
                </label>
                <select
                  value={selectedWarehouse}
                  onChange={(e) => setSelectedWarehouse(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  <option value="">Tất cả kho</option>
                  {warehouses.map((warehouse) => (
                    <option key={warehouse.id} value={warehouse.id}>
                      {warehouse.name}
                    </option>
                  ))}
                </select>
              </div>
            </div>
          </div>
        )}

        {/* Content */}
        {activeTab === 'items' && (
          <StockItemsTable
            stockItems={stockItems}
            isLoading={isLoading}
            getProductName={getProductName}
            getWarehouseName={getWarehouseName}
            canManageStock={canManageStock}
            onRefresh={loadStockItems}
          />
        )}

        {activeTab === 'transactions' && (
          <TransactionsTable
            transactions={transactions}
            isLoading={isLoading}
            getProductName={getProductName}
            getWarehouseName={getWarehouseName}
          />
        )}

        {activeTab === 'alerts' && (
          <LowStockAlert
            lowStockItems={lowStockItems}
            isLoading={isLoading}
          />
        )}
      </div>

      {/* Modals */}
      {showImportModal && (
        <ImportStockModal
          products={products}
          warehouses={warehouses}
          onClose={() => setShowImportModal(false)}
          onSuccess={() => {
            setShowImportModal(false);
            if (activeTab === 'items') loadStockItems();
            if (activeTab === 'transactions') loadTransactions();
          }}
        />
      )}

      {showExportModal && (
        <ExportStockModal
          products={products}
          warehouses={warehouses}
          onClose={() => setShowExportModal(false)}
          onSuccess={() => {
            setShowExportModal(false);
            if (activeTab === 'items') loadStockItems();
            if (activeTab === 'transactions') loadTransactions();
          }}
        />
      )}

      {showAdjustModal && (
        <AdjustStockModal
          products={products}
          warehouses={warehouses}
          onClose={() => setShowAdjustModal(false)}
          onSuccess={() => {
            setShowAdjustModal(false);
            if (activeTab === 'items') loadStockItems();
            if (activeTab === 'transactions') loadTransactions();
          }}
        />
      )}
    </div>
  );
};

// Stock Items Table Component
const StockItemsTable = ({ stockItems, isLoading, getProductName, getWarehouseName, canManageStock, onRefresh }) => {
  if (isLoading) {
    return (
      <div className="bg-white rounded-lg shadow-md p-8">
        <LoadingSpinner size="medium" text="Đang tải tồn kho..." />
      </div>
    );
  }

  if (stockItems.length === 0) {
    return (
      <div className="bg-white rounded-lg shadow-md p-8 text-center">
        <p className="text-gray-500">Không có dữ liệu tồn kho</p>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-md overflow-hidden">
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Sản phẩm
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Kho
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Số lượng
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Giới hạn
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Trạng thái
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Cập nhật lần cuối
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {stockItems.map((item) => (
              <tr key={item.id} className="hover:bg-gray-50">
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="text-sm font-medium text-gray-900">
                    {getProductName(item.productId)}
                  </div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                  {getWarehouseName(item.warehouseId)}
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="text-sm font-medium text-gray-900">
                    {item.quantity}
                  </div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {item.minQuantity} - {item.maxQuantity}
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                    item.quantity <= item.minQuantity
                      ? 'bg-red-100 text-red-800'
                      : item.quantity >= item.maxQuantity
                      ? 'bg-yellow-100 text-yellow-800'
                      : 'bg-green-100 text-green-800'
                  }`}>
                    {item.quantity <= item.minQuantity
                      ? 'Sắp hết'
                      : item.quantity >= item.maxQuantity
                      ? 'Dư thừa'
                      : 'Bình thường'
                    }
                  </span>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {formatDateTime(item.lastUpdated)}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

// Transactions Table Component
const TransactionsTable = ({ transactions, isLoading, getProductName, getWarehouseName }) => {
  if (isLoading) {
    return (
      <div className="bg-white rounded-lg shadow-md p-8">
        <LoadingSpinner size="medium" text="Đang tải lịch sử giao dịch..." />
      </div>
    );
  }

  if (transactions.length === 0) {
    return (
      <div className="bg-white rounded-lg shadow-md p-8 text-center">
        <p className="text-gray-500">Không có giao dịch nào</p>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-md overflow-hidden">
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Loại
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Sản phẩm
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Kho
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Số lượng
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Thời gian
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Ghi chú
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {transactions.map((transaction) => (
              <tr key={transaction.id} className="hover:bg-gray-50">
                <td className="px-6 py-4 whitespace-nowrap">
                  <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                    transaction.transactionType === 'Import'
                      ? 'bg-green-100 text-green-800'
                      : transaction.transactionType === 'Export'
                      ? 'bg-blue-100 text-blue-800'
                      : 'bg-yellow-100 text-yellow-800'
                  }`}>
                    {getTransactionTypeDisplayName(transaction.transactionType)}
                  </span>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                  {getProductName(transaction.productId)}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                  {getWarehouseName(transaction.warehouseId)}
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className={`text-sm font-medium ${
                    transaction.transactionType === 'Export' ? 'text-red-600' : 'text-green-600'
                  }`}>
                    {transaction.transactionType === 'Export' ? '-' : '+'}{transaction.quantity}
                  </div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {formatDateTime(transaction.transactionDate)}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {transaction.notes || '-'}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

// Low Stock Alert Component
const LowStockAlert = ({ lowStockItems, isLoading }) => {
  if (isLoading) {
    return (
      <div className="bg-white rounded-lg shadow-md p-8">
        <LoadingSpinner size="medium" text="Đang tải cảnh báo..." />
      </div>
    );
  }

  if (lowStockItems.length === 0) {
    return (
      <div className="bg-white rounded-lg shadow-md p-8 text-center">
        <div className="text-green-600">
          <span className="text-4xl">✅</span>
          <p className="mt-2 text-lg font-medium">Tuyệt vời!</p>
          <p className="text-gray-600">Không có sản phẩm nào sắp hết hàng</p>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-md overflow-hidden">
      <div className="bg-red-50 px-6 py-4 border-b">
        <h3 className="text-lg font-medium text-red-800 flex items-center">
          <span className="mr-2">⚠️</span>
          Cảnh báo hàng sắp hết ({lowStockItems.length} sản phẩm)
        </h3>
      </div>
      
      <div className="divide-y divide-gray-200">
        {lowStockItems.map((item) => (
          <div key={item.id} className="px-6 py-4 flex items-center justify-between hover:bg-gray-50">
            <div>
              <h4 className="text-sm font-medium text-gray-900">{item.productName}</h4>
              <p className="text-sm text-gray-500">{item.warehouseName}</p>
            </div>
            <div className="text-right">
              <div className="text-sm font-medium text-red-600">
                {item.quantity} / {item.minQuantity}
              </div>
              <div className="text-xs text-gray-500">Hiện tại / Tối thiểu</div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

// Import Stock Modal
const ImportStockModal = ({ products, warehouses, onClose, onSuccess }) => {
  const [formData, setFormData] = useState({
    productId: '',
    warehouseId: '',
    quantity: '',
    notes: ''
  });
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      setIsLoading(true);
      setError('');
      
      await stockApi.importStock({
        ...formData,
        quantity: parseInt(formData.quantity),
        userId: '' // Will be set by API from token
      });
      
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
        <h3 className="text-lg font-medium text-gray-900 mb-4 flex items-center">
          <span className="mr-2">📥</span>
          Nhập kho
        </h3>
        
        {error && (
          <div className="mb-4 bg-red-50 border border-red-200 rounded-md p-3">
            <p className="text-red-800 text-sm">{error}</p>
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Sản phẩm *
              </label>
              <select
                name="productId"
                required
                value={formData.productId}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="">Chọn sản phẩm</option>
                {products.map((product) => (
                  <option key={product.id} value={product.id}>
                    {product.name} ({product.sku})
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Kho *
              </label>
              <select
                name="warehouseId"
                required
                value={formData.warehouseId}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="">Chọn kho</option>
                {warehouses.map((warehouse) => (
                  <option key={warehouse.id} value={warehouse.id}>
                    {warehouse.name}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Số lượng *
              </label>
              <input
                type="number"
                name="quantity"
                required
                min="1"
                value={formData.quantity}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Ghi chú
              </label>
              <textarea
                name="notes"
                rows="3"
                value={formData.notes}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Ghi chú về lô hàng nhập..."
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
              className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 transition-colors disabled:opacity-50"
            >
              {isLoading ? 'Đang nhập...' : 'Nhập kho'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

// Export Stock Modal
const ExportStockModal = ({ products, warehouses, onClose, onSuccess }) => {
  const [formData, setFormData] = useState({
    productId: '',
    warehouseId: '',
    quantity: '',
    notes: ''
  });
  const [availableStock, setAvailableStock] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  // Check available stock when product and warehouse are selected
  useEffect(() => {
    if (formData.productId && formData.warehouseId) {
      checkAvailableStock();
    } else {
      setAvailableStock(null);
    }
  }, [formData.productId, formData.warehouseId]);

  const checkAvailableStock = async () => {
    try {
      const result = await stockApi.getAvailableStock(formData.productId, formData.warehouseId);
      setAvailableStock(result.availableQuantity);
    } catch (error) {
      console.error('Error checking available stock:', error);
      setAvailableStock(0);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      setIsLoading(true);
      setError('');
      
      const quantity = parseInt(formData.quantity);
      
      if (quantity > availableStock) {
        setError(`Số lượng xuất (${quantity}) vượt quá tồn kho hiện tại (${availableStock})`);
        return;
      }
      
      await stockApi.exportStock({
        ...formData,
        quantity,
        userId: '' // Will be set by API from token
      });
      
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
        <h3 className="text-lg font-medium text-gray-900 mb-4 flex items-center">
          <span className="mr-2">📤</span>
          Xuất kho
        </h3>
        
        {error && (
          <div className="mb-4 bg-red-50 border border-red-200 rounded-md p-3">
            <p className="text-red-800 text-sm">{error}</p>
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Sản phẩm *
              </label>
              <select
                name="productId"
                required
                value={formData.productId}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="">Chọn sản phẩm</option>
                {products.map((product) => (
                  <option key={product.id} value={product.id}>
                    {product.name} ({product.sku})
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Kho *
              </label>
              <select
                name="warehouseId"
                required
                value={formData.warehouseId}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="">Chọn kho</option>
                {warehouses.map((warehouse) => (
                  <option key={warehouse.id} value={warehouse.id}>
                    {warehouse.name}
                  </option>
                ))}
              </select>
            </div>

            {availableStock !== null && (
              <div className="bg-blue-50 p-3 rounded-md">
                <p className="text-sm text-blue-800">
                  Tồn kho hiện tại: <span className="font-medium">{availableStock}</span>
                </p>
              </div>
            )}

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Số lượng xuất *
              </label>
              <input
                type="number"
                name="quantity"
                required
                min="1"
                max={availableStock || undefined}
                value={formData.quantity}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Ghi chú
              </label>
              <textarea
                name="notes"
                rows="3"
                value={formData.notes}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Lý do xuất kho..."
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
              disabled={isLoading || !availableStock || parseInt(formData.quantity) > availableStock}
              className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors disabled:opacity-50"
            >
              {isLoading ? 'Đang xuất...' : 'Xuất kho'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

// Adjust Stock Modal
const AdjustStockModal = ({ products, warehouses, onClose, onSuccess }) => {
  const [formData, setFormData] = useState({
    productId: '',
    warehouseId: '',
    quantity: '',
    notes: ''
  });
  const [currentStock, setCurrentStock] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  // Check current stock when product and warehouse are selected
  useEffect(() => {
    if (formData.productId && formData.warehouseId) {
      checkCurrentStock();
    } else {
      setCurrentStock(null);
    }
  }, [formData.productId, formData.warehouseId]);

  const checkCurrentStock = async () => {
    try {
      const result = await stockApi.getAvailableStock(formData.productId, formData.warehouseId);
      setCurrentStock(result.availableQuantity);
    } catch (error) {
      console.error('Error checking current stock:', error);
      setCurrentStock(0);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      setIsLoading(true);
      setError('');
      
      await stockApi.adjustStock({
        ...formData,
        quantity: parseInt(formData.quantity),
        userId: '' // Will be set by API from token
      });
      
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

  const adjustment = currentStock !== null && formData.quantity 
    ? parseInt(formData.quantity) - currentStock 
    : 0;

  return (
    <div className="fixed inset-0 bg-gray-600 bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-6 w-full max-w-md">
        <h3 className="text-lg font-medium text-gray-900 mb-4 flex items-center">
          <span className="mr-2">⚖️</span>
          Điều chỉnh tồn kho
        </h3>
        
        {error && (
          <div className="mb-4 bg-red-50 border border-red-200 rounded-md p-3">
            <p className="text-red-800 text-sm">{error}</p>
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Sản phẩm *
              </label>
              <select
                name="productId"
                required
                value={formData.productId}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="">Chọn sản phẩm</option>
                {products.map((product) => (
                  <option key={product.id} value={product.id}>
                    {product.name} ({product.sku})
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Kho *
              </label>
              <select
                name="warehouseId"
                required
                value={formData.warehouseId}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="">Chọn kho</option>
                {warehouses.map((warehouse) => (
                  <option key={warehouse.id} value={warehouse.id}>
                    {warehouse.name}
                  </option>
                ))}
              </select>
            </div>

            {currentStock !== null && (
              <div className="bg-gray-50 p-3 rounded-md">
                <p className="text-sm text-gray-800">
                  Tồn kho hiện tại: <span className="font-medium">{currentStock}</span>
                </p>
                {formData.quantity && (
                  <p className="text-sm text-gray-600 mt-1">
                    Điều chỉnh: <span className={`font-medium ${adjustment >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                      {adjustment >= 0 ? '+' : ''}{adjustment}
                    </span>
                  </p>
                )}
              </div>
            )}

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Số lượng mới *
              </label>
              <input
                type="number"
                name="quantity"
                required
                min="0"
                value={formData.quantity}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Lý do điều chỉnh *
              </label>
              <textarea
                name="notes"
                rows="3"
                required
                value={formData.notes}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Lý do điều chỉnh tồn kho..."
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
              className="px-4 py-2 bg-yellow-600 text-white rounded-md hover:bg-yellow-700 transition-colors disabled:opacity-50"
            >
              {isLoading ? 'Đang điều chỉnh...' : 'Điều chỉnh'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default StockPage;