import { useState } from "react";
import ProviderResult from "../../DTO/Contexts/ProviderResult";
import UserProfileInfo from "../../DTO/Domain/UserProfileInfo";
import IProfileProvider from "../../DTO/Contexts/IProfileProvider";
import ProfileFactory from "../../Business/Factory/ProfileFactory";
import ProfileContext from "./ProfileContext";

export default function ProfileProvider(props: any) {

    const [loading, setLoading] = useState<boolean>(false);
    const [loadingUpdate, setLoadingUpdate] = useState<boolean>(false);

    const [profile, _setProfile] = useState<UserProfileInfo>(null);
    const [profiles, setProfiles] = useState<UserProfileInfo[]>([]);

    const profileProviderValue: IProfileProvider = {
        loading: loading,
        loadingUpdate: loadingUpdate,

        profile: profile,
        profiles: profiles,

        setProfile: (profile: UserProfileInfo) => {
            _setProfile(profile);
        },

        insert: async (profile: UserProfileInfo) => {
            let ret: Promise<ProviderResult>;
            setLoadingUpdate(true);
            //try {
            let brt = await ProfileFactory.ProfileBusiness.insert(profile);
            if (brt.sucesso) {
                setLoadingUpdate(false);
                _setProfile(brt.dataResult);
                return {
                    ...ret,
                    sucesso: true,
                    mensagemSucesso: "Profile added"
                };
            }
            else {
                setLoadingUpdate(false);
                return {
                    ...ret,
                    sucesso: false,
                    mensagemErro: brt.mensagem
                };
            }
            /*
            }
            catch (err) {
                setLoadingUpdate(false);
                return {
                    ...ret,
                    sucesso: false,
                    mensagemErro: JSON.stringify(err)
                };
            }
            */
        },
        update: async (profile: UserProfileInfo) => {
            let ret: Promise<ProviderResult>;
            setLoadingUpdate(true);
            try {
                let brt = await ProfileFactory.ProfileBusiness.update(profile);
                if (brt.sucesso) {
                    setLoadingUpdate(false);
                    _setProfile(brt.dataResult);
                    return {
                        ...ret,
                        sucesso: true,
                        mensagemSucesso: "Profile updated"
                    };
                }
                else {
                    setLoadingUpdate(false);
                    return {
                        ...ret,
                        sucesso: false,
                        mensagemErro: brt.mensagem
                    };
                }
            }
            catch (err) {
                setLoadingUpdate(false);
                return {
                    ...ret,
                    sucesso: false,
                    mensagemErro: JSON.stringify(err)
                };
            }
        },
        delete: async (profileId: number) => {
            let ret: Promise<ProviderResult>;
            setLoadingUpdate(true);
            try {
                let brt = await ProfileFactory.ProfileBusiness.delete(profileId);
                if (brt.sucesso) {
                    setLoadingUpdate(false);
                    return {
                        ...ret,
                        sucesso: true,
                        mensagemSucesso: "Profile deleted"
                    };
                }
                else {
                    setLoadingUpdate(false);
                    return {
                        ...ret,
                        sucesso: false,
                        mensagemErro: brt.mensagem
                    };
                }
            }
            catch (err) {
                setLoadingUpdate(false);
                return {
                    ...ret,
                    sucesso: false,
                    mensagemErro: JSON.stringify(err)
                };
            }
        },
        listByNetwork: async (networkId: number) => {
            let ret: Promise<ProviderResult>;
            setLoading(true);
            try {
                let brt = await ProfileFactory.ProfileBusiness.listByNetwork(networkId);
                if (brt.sucesso) {
                    setLoading(false);
                    setProfiles(brt.dataResult);
                    return {
                        ...ret,
                        sucesso: true,
                        mensagemSucesso: "Network list"
                    };
                }
                else {
                    setLoading(false);
                    return {
                        ...ret,
                        sucesso: false,
                        mensagemErro: brt.mensagem
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
        getById: async (profileId: number) => {
            let ret: Promise<ProviderResult>;
            setLoading(true);
            try {
                let brt = await ProfileFactory.ProfileBusiness.getById(profileId);
                if (brt.sucesso) {
                    setLoading(false);
                    _setProfile(brt.dataResult);
                    return {
                        ...ret,
                        sucesso: true,
                        mensagemSucesso: "Load Profile"
                    };
                }
                else {
                    setLoading(false);
                    return {
                        ...ret,
                        sucesso: false,
                        mensagemErro: brt.mensagem
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
        }
    }

    return (
        <ProfileContext.Provider value={profileProviderValue}>
            {props.children}
        </ProfileContext.Provider>
    );
}