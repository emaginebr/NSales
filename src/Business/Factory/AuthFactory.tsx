import AuthBusiness from '../Impl/AuthBusiness';
import IAuthBusiness from '../Interfaces/IAuthBusiness';

const authBusinessImpl: IAuthBusiness = AuthBusiness;

const AuthFactory = {
  AuthBusiness: authBusinessImpl
};

export default AuthFactory;
