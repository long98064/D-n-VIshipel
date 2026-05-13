import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { jwtDecode } from 'jwt-decode';

export const useAuthStore = create(
  persist(
    (set) => ({
      token: null,
      user: null,
      isAuthenticated: false,

      setCredentials: (token) => {
        try {
          const decoded = jwtDecode(token);
          set({ 
            token: token, 
            user: {
                userId: decoded.sub || decoded.nameid,
                role: decoded.role || decoded.Role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'],
                branchId: decoded.branchId || decoded.BranchId || decoded.branchid
            }, 
            isAuthenticated: true 
          });
        } catch (error) {
          console.error("Token không hợp lệ", error);
          set({ token: null, user: null, isAuthenticated: false });
        }
      },

      // ✅ NEW: Phục hồi từ token khi page reload
      loadFromToken: (token) => {
        if (!token) {
          set({ token: null, user: null, isAuthenticated: false });
          return;
        }

        try {
          const decoded = jwtDecode(token);
          
          // ✅ Kiểm tra token hết hạn
          const expiresAt = decoded.exp * 1000; // Convert to milliseconds
          if (expiresAt < Date.now()) {
            console.warn("Token đã hết hạn");
            set({ token: null, user: null, isAuthenticated: false });
            return;
          }

          set({
            token: token,
            user: {
              userId: decoded.sub || decoded.nameid,
              role: decoded.role || decoded.Role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'],
              branchId: decoded.branchId || decoded.BranchId || decoded.branchid
            },
            isAuthenticated: true
          });
        } catch (error) {
          console.error("Lỗi khi phục hồi token:", error);
          set({ token: null, user: null, isAuthenticated: false });
        }
      },

      logout: () => {
        set({ token: null, user: null, isAuthenticated: false });
      }
    }),
    {
      name: 'auth-storage', // Key lưu vào LocalStorage
    }
  )
);