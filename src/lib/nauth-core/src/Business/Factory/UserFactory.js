import ServiceFactory from '../../Services/ServiceFactory';
import UserBusiness from '../Impl/UserBusiness';
const userService = ServiceFactory.UserService;
const userBusinessImpl = UserBusiness;
userBusinessImpl.init(userService);
const UserFactory = {
    UserBusiness: userBusinessImpl,
};
export default UserFactory;
