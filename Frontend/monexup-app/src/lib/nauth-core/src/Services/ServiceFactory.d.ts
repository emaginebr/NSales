import IUserService from './Interfaces/IUserService';
declare const ServiceFactory: {
    UserService: IUserService;
    setLogoffCallback: (cb: () => void) => void;
};
export default ServiceFactory;
