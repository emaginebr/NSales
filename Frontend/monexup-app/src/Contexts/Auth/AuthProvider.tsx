import React, { useContext, useState } from 'react';
import ProviderResult from '../../DTO/Contexts/ProviderResult';
import IAuthProvider from '../../DTO/Contexts/IAuthProvider';
import AuthContext from './AuthContext';
import AuthFactory from '../../Business/Factory/AuthFactory';
import AuthSession from '../../DTO/Domain/AuthSession';
import UserFactory from '../../Business/Factory/UserFactory';
import UserInfo from '../../DTO/Domain/UserInfo';
import { LanguageEnum } from '../../DTO/Enum/LanguageEnum';

export default function AuthProvider(props: any) {

  const [loading, setLoading] = useState(false);
  const [language, setLanguage] = useState<LanguageEnum>(LanguageEnum.English);
  const [sessionInfo, _setSessionInfo] = useState<AuthSession>(null);

  const authProviderValue: IAuthProvider = {
    loading: loading,
    language: language,
    sessionInfo: sessionInfo,

    setSession: (session: AuthSession) => {
      console.log(JSON.stringify(session));
      _setSessionInfo(session);
      AuthFactory.AuthBusiness.setSession(session);
    },
    setLanguage: (value: LanguageEnum) => {
      setLanguage(value);
    },
    loginWithEmail: async (email: string, password: string) => {
      let ret: Promise<ProviderResult>;
      setLoading(true);
      try {
        let retLog = await UserFactory.UserBusiness.loginWithEmail(email, password);
        if (retLog.sucesso) {
          let retTok = await UserFactory.UserBusiness.getTokenAuthorized(email, password);
          if (retTok.sucesso) {
            authProviderValue.setSession({
              ...sessionInfo,
              userId: retLog.dataResult.userId,
              hash: retLog.dataResult.hash,
              token: retTok.dataResult,
              isAdmin: retLog.dataResult.isAdmin,
              name: retLog.dataResult.name,
              email: retLog.dataResult.email,
              language: language
            });
            setLoading(false);
            return {
              ...ret,
              sucesso: true,
              mensagemSucesso: "User Logged"
            };
          }
          else {
            setLoading(false);
            return {
              ...ret,
              sucesso: false,
              mensagemErro: retTok.mensagem
            };
          }
        }
        else {
          setLoading(false);
          return {
            ...ret,
            sucesso: false,
            mensagemErro: retLog.mensagem
          };
        }
      }
      catch (err) {
        setLoading(false);
        return {
          ...ret,
          sucesso: false,
          mensagemErro: JSON.stringify(err)
        };
      }
    },
    logout: function (): ProviderResult {
      try {
        AuthFactory.AuthBusiness.cleanSession();
        _setSessionInfo(null);
        return {
          sucesso: true,
          mensagemErro: "",
          mensagemSucesso: ""
        };
      } catch (err) {
        return {
          sucesso: false,
          mensagemErro: "Falha ao tenta executar o logout",
          mensagemSucesso: ""
        };
      }
    },
    loadUserSession: async () => {
      let ret: Promise<ProviderResult>;
      let session = await AuthFactory.AuthBusiness.getSession();
      if (session) {
        authProviderValue.setSession(session);
        return {
          ...ret,
          sucesso: true
        };
      }
      return {
        ...ret,
        sucesso: false,
        mensagemErro: "Session not load"
      };
    }
  };

  return (
    <AuthContext.Provider value={authProviderValue}>
      {props.children}
    </AuthContext.Provider>
  );
}
