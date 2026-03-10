import ServiceFactory from '../../Services/ServiceFactory';
import OrderBusiness from '../Impl/OrderBusiness';
import IOrderBusiness from '../Interfaces/IOrderBusiness';

const orderService = ServiceFactory.OrderService;

const orderBusinessImpl: IOrderBusiness = OrderBusiness;
orderBusinessImpl.init(orderService);

const OrderFactory = {
  OrderBusiness: orderBusinessImpl
};

export default OrderFactory;
