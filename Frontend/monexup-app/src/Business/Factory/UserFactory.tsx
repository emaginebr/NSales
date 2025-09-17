import ServiceFactory from '../../Services/ServiceFactory';
import UserBusiness from '../Impl/UserBusiness';
import IUserBusiness from '../Interfaces/IUserBusiness';

const userService = ServiceFactory.UserService;

const userBusinessImpl: IUserBusiness = UserBusiness;
userBusinessImpl.init(userService);

const UserFactory = {
  UserBusiness: userBusinessImpl
};

export default UserFactory;
