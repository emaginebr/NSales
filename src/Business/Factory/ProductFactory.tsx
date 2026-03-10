import ServiceFactory from '../../Services/ServiceFactory';
import ProductBusiness from '../Impl/ProductBusiness';
import IProductBusiness from '../Interfaces/IProductBusiness';

const productService = ServiceFactory.ProductService;

const productBusinessImpl: IProductBusiness = ProductBusiness;
productBusinessImpl.init(productService);

const ProductFactory = {
  ProductBusiness: productBusinessImpl
};

export default ProductFactory;
