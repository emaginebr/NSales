import ServiceFactory from '../../Services/ServiceFactory';
import NetworkBusiness from '../Impl/NetworkBusiness';
import INetworkBusiness from '../Interfaces/INetworkBusiness';

const networlService = ServiceFactory.NetworkService;

const networkBusinessImpl: INetworkBusiness = NetworkBusiness;
networkBusinessImpl.init(networlService);

const NetworkFactory = {
  NetworkBusiness: networkBusinessImpl
};

export default NetworkFactory;
