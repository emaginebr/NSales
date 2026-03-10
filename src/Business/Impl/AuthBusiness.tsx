import AuthSession from "../../DTO/Domain/AuthSession";
import IAuthBusiness from "../Interfaces/IAuthBusiness";

const LS_KEY = 'login-with-metamask:auth';

const AuthBusiness : IAuthBusiness = {
  getSession: () => {
    const ls = window.localStorage.getItem(LS_KEY);
    return ls && JSON.parse(ls);
  },
  setSession: (session: AuthSession) => {
    console.log("Set Session: ", JSON.stringify(session));
    localStorage.setItem(LS_KEY, JSON.stringify(session));
  },
  cleanSession: () => {
    localStorage.removeItem(LS_KEY);
  }
}

export default AuthBusiness;