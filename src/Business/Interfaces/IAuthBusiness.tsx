import BusinessResult from "../../DTO/Business/BusinessResult";
import AuthSession from "../../DTO/Domain/AuthSession";

export default interface IAuthBusiness {
  getSession: () => AuthSession;
  setSession: (session: AuthSession) => void;
  cleanSession: () => void;
}