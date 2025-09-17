import { HttpClient } from '../Infra/Impl/HttpClient';
import UserService from './Impl/UserService';
const httpClientAuth = HttpClient();
console.log("VITE_NAUTH_URL", import.meta.env.VITE_NAUTH_URL);
httpClientAuth.init(import.meta.env.VITE_NAUTH_URL);
const userServiceImpl = UserService;
userServiceImpl.init(httpClientAuth);
const ServiceFactory = {
    UserService: userServiceImpl,
    setLogoffCallback: (cb) => {
        httpClientAuth.setLogoff(cb);
    },
};
export default ServiceFactory;
