import { HttpClient } from '../Infra/Impl/HttpClient';
import IHttpClient from '../Infra/Interface/IHttpClient';
import IUserService from './Interfaces/IUserService';
import UserService from './Impl/UserService';
import INetworkService from './Interfaces/INetworkService';
import NetworkService from './Impl/NetworkService';
import IProfileService from './Interfaces/IProfileService';
import ProfileService from './Impl/ProfileService';
import IProductService from './Interfaces/IProductService';
import ProductService from './Impl/ProductService';
import IOrderService from './Interfaces/IOrderService';
import OrderService from './Impl/OrderService';
import IInvoiceService from './Interfaces/IInvoiceService';
import InvoiceService from './Impl/InvoiceService';
import IImageService from './Interfaces/IImageService';
import ImageService from './Impl/ImageService';
import ITemplateService from './Interfaces/ITemplateService';
import TemplateService from './Impl/TemplateService';

const httpClientAuth : IHttpClient = HttpClient();
httpClientAuth.init(process.env.REACT_APP_API_URL);

const userServiceImpl : IUserService = UserService;
userServiceImpl.init(httpClientAuth);

const networkServiceImpl : INetworkService = NetworkService;
networkServiceImpl.init(httpClientAuth);

const profileServiceImpl : IProfileService = ProfileService;
profileServiceImpl.init(httpClientAuth);

const productServiceImpl : IProductService = ProductService;
productServiceImpl.init(httpClientAuth);

const orderServiceImpl : IOrderService = OrderService;
orderServiceImpl.init(httpClientAuth);

const invoiceServiceImpl : IInvoiceService = InvoiceService;
invoiceServiceImpl.init(httpClientAuth);

const imageServiceImpl : IImageService = ImageService;
imageServiceImpl.init(httpClientAuth);

const templateServiceImpl : ITemplateService = TemplateService;
templateServiceImpl.init(httpClientAuth);

const ServiceFactory = {
  UserService: userServiceImpl,
  NetworkService: networkServiceImpl,
  ProfileService: profileServiceImpl,
  ProductService: productServiceImpl,
  OrderService: orderServiceImpl,
  InvoiceService: invoiceServiceImpl,
  ImageService: imageServiceImpl,
  TemplateService: templateServiceImpl,
  setLogoffCallback: (cb : () => void) => {
    httpClientAuth.setLogoff(cb);
  }
};

export default ServiceFactory;