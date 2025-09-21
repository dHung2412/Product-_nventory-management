import React, { createContext, useContext, useState, useEffect } from 'react';
import authApi from '../api/authApi';
import { 
  getToken, 
  setToken, 
  getRefreshToken, 
  setRefreshToken, 
  getUser, 
  setUser, 
  clearAuthData 
} from '../utils/storage';

const AuthContext = createContext();

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

export const AuthProvider = ({ children }) => {
  const [user, setCurrentUser] = useState(null);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  // Kiểm tra auth state khi component mount
  useEffect(() => {
    initializeAuth();
  }, []);

  const initializeAuth = async () => {
    try {
      const token = getToken();
      const savedUser = getUser();
      
      if (token && savedUser) {
        // Validate token với server
        const isValid = await authApi.validateToken();
        if (isValid) {
          setCurrentUser(savedUser);
          setIsAuthenticated(true);
        } else {
          clearAuthData();
        }
      }
    } catch (error) {
      console.error('Auth initialization failed:', error);
      clearAuthData();
    } finally {
      setIsLoading(false);
    }
  };

  const login = async (loginData) => {
    try {
      const response = await authApi.login(loginData);
      const { accessToken, refreshToken, user } = response;
      console.log("User from login response:", user);
      // Lưu thông tin auth
      setToken(accessToken);
      setRefreshToken(refreshToken);
      setUser(user);
      
      // Update state
      setCurrentUser(user);
      setIsAuthenticated(true);
      
      return { success: true, user };
    } catch (error) {
      console.error('Login failed:', error);
      return { success: false, error: error.message };
    }
  };

  const register = async (registerData) => {
    try {
      const response = await authApi.register(registerData);
      const { accessToken, refreshToken, user } = response.data;
      
      // Lưu thông tin auth
      setToken(accessToken);
      setRefreshToken(refreshToken);
      setUser(user);
      
      // Update state
      setCurrentUser(user);
      setIsAuthenticated(true);
      
      return { success: true, user };
    } catch (error) {
      console.error('Registration failed:', error);
      return { success: false, error: error.message };
    }
  };

  const logout = async () => {
    try {
      const refreshToken = getRefreshToken();
      if (refreshToken) {
        await authApi.logout(refreshToken);
      }
    } catch (error) {
      console.error('Logout API call failed:', error);
    } finally {
      // Clear auth data regardless of API call result
      clearAuthData();
      setCurrentUser(null);
      setIsAuthenticated(false);
    }
  };

  const refreshAuthToken = async () => {
    try {
      const refreshToken = getRefreshToken();
      if (!refreshToken) {
        throw new Error('No refresh token available');
      }
      
      const response = await authApi.refreshToken(refreshToken);
      const { accessToken, refreshToken: newRefreshToken } = response;
      
      setToken(accessToken);
      if (newRefreshToken) {
        setRefreshToken(newRefreshToken);
      }
      
      return accessToken;
    } catch (error) {
      console.error('Token refresh failed:', error);
      await logout();
      throw error;
    }
  };

  const updateUser = (updatedUser) => {
    setUser(updatedUser);
    setCurrentUser(updatedUser);
  };

  const hasRole = (roles) => {
    if (!user || !roles) return false;
    if (typeof roles === 'string') {
      return user.role === roles;
    }
    return roles.includes(user.role);
  };

  const value = {
    user,
    isAuthenticated,
    isLoading,
    login,
    register,
    logout,
    refreshAuthToken,
    updateUser,
    hasRole,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};