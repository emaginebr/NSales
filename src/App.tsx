import './App.css';
import { Routes, Route, Outlet, Link } from "react-router-dom";
import Menu from "./Components/Menu";
import ContextBuilder from './Contexts/Utils/ContextBuilder';
import AuthProvider from './Contexts/Auth/AuthProvider';
import UserPage from './Pages/UserPage';
import PasswordPage from './Pages/PasswordPage';
import LoginPage from './Pages/LoginPage';
import RecoveryPage from './Pages/RecoveryPage';
import UserProvider from './Contexts/User/UserProvider';
import HomePage from './Pages/HomePage';
import NetworkPage from './Pages/NetworkPage';
import DashboardPage from './Pages/DashboardPage';
import NetworkEditPage from './Pages/NetworkEditPage';
import NetworkListPage from './Pages/NetworkListPage';
import ProductEditPage from './Pages/ProductEditPage';
import ProductPage from './Pages/ProductPage';
import NetworkInsertPage from './Pages/NetworkInsertPage';
import NetworkProvider from './Contexts/Network/NetworkProvider';
import ProfileProvider from './Contexts/Profile/ProfileProvider';
import ProfileListPage from './Pages/ProfileListPage';
import ProfileEditPage from './Pages/ProfileEditPage';
import UserSearchPage from './Pages/UserSearchPage';
import ProductProvider from './Contexts/Product/ProductProvider';
import ProductSearchPage from './Pages/ProductSearchPage';
import MenuNetwork from './Components/MenuNetwork';
import RequestAccessPage from './Pages/RequestAccessPage';
import OrderProvider from './Contexts/Order/OrderProvider';
import Error404Page from './Pages/Error404Page';
import OrderSearchPage from './Pages/OrderSearchPage';
import SellerAddPage from './Pages/SellerAddPage';
import SellerPage from './Pages/SellerPage';
import InvoiceProvider from './Contexts/Invoice/InvoiceProvider';
import InvoiceSearchPage from './Pages/InvoiceSearchPage';
import NetworkFooter from './Pages/NetworkPage/NetworkFooter';
import MenuUser from './Components/MenuUser';
import ImageProvider from './Contexts/Image/ImageProvider';
import TemplateProvider from './Contexts/Template/TemplateProvider';

function Layout() {
  return (
    <div>
      <Menu />
      <Outlet />
    </div>
  );
}

function LayoutNetwork() {
  return (
    <div>
      <MenuNetwork />
      <Outlet />
      <NetworkFooter />
    </div>
  );
}

function LayoutUser() {
  return (
    <div>
      <MenuUser />
      <Outlet />
      <NetworkFooter />
    </div>
  );
}

function App() {
  const ContextContainer = ContextBuilder([
    AuthProvider, UserProvider, NetworkProvider, ProfileProvider, ProductProvider, 
    OrderProvider, InvoiceProvider, ImageProvider, TemplateProvider
  ]);

  return (
    <ContextContainer>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<HomePage />} />
          <Route path="new-seller" element={<SellerAddPage />} />
          <Route path="network">
            <Route index element={<NetworkInsertPage />} />
            <Route path="search" element={<NetworkListPage />} />
          </Route>
          <Route path="account">
            <Route index element={<LoginPage />} />
            <Route path="login" element={<LoginPage />} />
            <Route path="edit-account" element={<UserPage />} />
            <Route path="new-account" element={<UserPage />} />
            <Route path="recovery-password" element={<RecoveryPage />} />
            <Route path="change-password" element={<PasswordPage />} />
          </Route>
          <Route path="admin">
            <Route index element={<DashboardPage />} />
            <Route path="dashboard" element={<DashboardPage />} />
            <Route path="network" element={<NetworkEditPage />} />
            <Route path="teams" element={<UserSearchPage />} />
            <Route path="teams">
              <Route index element={<UserSearchPage />} />
              <Route path=":pageNum" element={<UserSearchPage />} />
            </Route>
            <Route path="orders" element={<OrderSearchPage />} />
            <Route path="invoices" element={<InvoiceSearchPage />} />
            <Route path="products">
              <Route index element={<ProductSearchPage />} />
              <Route path="new" element={<ProductEditPage />} />
              <Route path=":productId" element={<ProductEditPage />} />
            </Route>
            <Route path="p">
              <Route path=":id" element={<ProductPage />} />
            </Route>
            <Route path="team-structure">
              <Route index element={<ProfileListPage />} />
              <Route path="new" element={<ProfileEditPage />} />
              <Route path=":profileId" element={<ProfileEditPage />} />
            </Route>
          </Route>
        </Route>
        <Route path="@" element={<LayoutUser />}>
          <Route path=":sellerSlug">
            <Route index element={<SellerPage />} />
            <Route path="account">
              <Route index element={<LoginPage />} />
              <Route path="login" element={<LoginPage />} />
              <Route path="edit-account" element={<UserPage />} />
              <Route path="new-account" element={<UserPage />} />
              <Route path="recovery-password" element={<RecoveryPage />} />
              <Route path="change-password" element={<PasswordPage />} />
            </Route>
            <Route path=":productSlug" element={<ProductPage />} />
          </Route>
          <Route index element={<Error404Page />} />
        </Route>
        <Route path=":networkSlug" element={<LayoutNetwork />}>
          <Route index element={<NetworkPage />} />
          <Route path="new-seller" element={<SellerAddPage />} />
          <Route path="request-access" element={<RequestAccessPage />} />
          <Route path="account">
            <Route index element={<LoginPage />} />
            <Route path="login" element={<LoginPage />} />
            <Route path="edit-account" element={<UserPage />} />
            <Route path="new-account" element={<UserPage />} />
            <Route path="recovery-password" element={<RecoveryPage />} />
            <Route path="change-password" element={<PasswordPage />} />
          </Route>
          <Route path=":productSlug" element={<ProductPage />} />
          <Route path="@">
            <Route path=":sellerSlug">
              <Route index element={<SellerPage />} />
              <Route path="new-seller" element={<SellerAddPage />} />
              <Route path="request-access" element={<RequestAccessPage />} />
              <Route path=":productSlug" element={<ProductPage />} />
            </Route>
            <Route index element={<Error404Page />} />
          </Route>
        </Route>
        <Route path="*" element={<Error404Page />} />
      </Routes>
    </ContextContainer>
  );
}

export default App;
