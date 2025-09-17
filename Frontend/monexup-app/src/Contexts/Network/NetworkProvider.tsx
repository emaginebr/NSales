import { useState } from "react";
import ProviderResult from "../../DTO/Contexts/ProviderResult";
import UserFactory from "../../Business/Factory/UserFactory";
import NetworkContext from "./NetworkContext";
import INetworkProvider from "../../DTO/Contexts/INetworkProvider";
import NetworkInfo from "../../DTO/Domain/NetworkInfo";
import UserNetworkInfo from "../../DTO/Domain/UserNetworkInfo";
import NetworkInsertInfo from "../../DTO/Domain/NetworkInsertInfo";
import NetworkFactory from "../../Business/Factory/NetworkFactory";
import { UserRoleEnum } from "../../DTO/Enum/UserRoleEnum";
import NetworkProviderResult from "../../DTO/Contexts/NetworkProviderResult";

export default function NetworkProvider(props: any) {

    const [loading, setLoading] = useState<boolean>(false);
    const [loadingTeam, setLoadingTeam] = useState<boolean>(false);
    const [loadingSeller, setLoadingSeller] = useState<boolean>(false);
    const [loadingUpdate, setLoadingUpdate] = useState<boolean>(false);
    const [loadingRequestAccess, setLoadingRequestAccess] = useState<boolean>(false);

    const [network, _setNetwork] = useState<NetworkInfo>(null);
    const [networks, setNetworks] = useState<NetworkInfo[]>([]);
    const [userNetwork, _setUserNetwork] = useState<UserNetworkInfo>(null);
    const [seller, setSeller] = useState<UserNetworkInfo>(null);
    const [userNetworks, setUserNetworks] = useState<UserNetworkInfo[]>([]);
    const [teams, setTeams] = useState<UserNetworkInfo[]>([]);
    const [currentRole, _setCurrentRole] = useState<UserRoleEnum>(UserRoleEnum.User);

    const networkProviderValue: INetworkProvider = {
        loading: loading,
        loadingTeam: loadingTeam,
        loadingSeller: loadingSeller,
        loadingUpdate: loadingUpdate,
        loadingRequestAccess: loadingRequestAccess,

        network: network,
        networks: networks,
        userNetwork: userNetwork,
        seller: seller,
        userNetworks: userNetworks,
        teams: teams,
        currentRole: currentRole,

        setNetwork: (network: NetworkInfo) => {
            _setNetwork(network);
        },
        setUserNetwork: (userNetwork: UserNetworkInfo) => {
            _setUserNetwork(userNetwork);
            _setNetwork(userNetwork?.network);
            _setCurrentRole(userNetwork.role);
        },
        setCurrentRole: (role: UserRoleEnum) => {
            _setCurrentRole(role);
        },
        /*
        setEditMode: (edit: boolean) => {
            if (edit) {
                if (currentRole >= UserRoleEnum.NetworkManager) {
                    _setEditMode(true);                
                }
                else {
                    _setEditMode(false);
                }
            }
            else {
                _setEditMode(false);
            }
        },
        */

        insert: async (network: NetworkInsertInfo) => {
            let ret: Promise<ProviderResult>;
            setLoadingUpdate(true);
            //try {
            let brt = await NetworkFactory.NetworkBusiness.insert(network);
            if (brt.sucesso) {
                setLoadingUpdate(false);
                _setNetwork(brt.dataResult);
                return {
                    ...ret,
                    sucesso: true,
                    mensagemSucesso: "Network added"
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
        update: async (network: NetworkInfo) => {
            let ret: Promise<ProviderResult>;
            setLoadingUpdate(true);
            try {
                let brt = await NetworkFactory.NetworkBusiness.update(network);
                if (brt.sucesso) {
                    setLoadingUpdate(false);
                    _setNetwork(brt.dataResult);
                    return {
                        ...ret,
                        sucesso: true,
                        mensagemSucesso: "User updated"
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
        listAll: async () => {
            let ret: Promise<ProviderResult>;
            setLoading(true);
            try {
                let brt = await NetworkFactory.NetworkBusiness.listAll();
                if (brt.sucesso) {
                    setLoading(false);
                    setNetworks(brt.dataResult);
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
        listByNetwork: async (networkSlug: string) => {
            let ret: Promise<ProviderResult>;
            setLoadingTeam(true);
            try {
                let brt = await NetworkFactory.NetworkBusiness.listByNetwork(networkSlug);
                if (brt.sucesso) {
                    setLoadingTeam(false);
                    setTeams(brt.dataResult);
                    return {
                        ...ret,
                        sucesso: true,
                        mensagemSucesso: "Network list"
                    };
                }
                else {
                    setLoadingTeam(false);
                    return {
                        ...ret,
                        sucesso: false,
                        mensagemErro: brt.mensagem
                    };
                }
            }
            catch (err) {
                setLoadingTeam(false);
                return {
                    ...ret,
                    sucesso: false,
                    mensagemErro: JSON.stringify(err)
                };
            }
        },
        listByUser: async () => {
            let ret: Promise<ProviderResult>;
            setLoading(true);
            try {
                let brt = await NetworkFactory.NetworkBusiness.listByUser();
                if (brt.sucesso) {
                    setLoading(false);
                    setUserNetworks(brt.dataResult);
                    if (!userNetwork) {
                        if (brt.dataResult.length > 0) {
                            _setUserNetwork(brt.dataResult[0]);
                            _setNetwork(brt.dataResult[0].network);
                        }
                    }
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
        getById: async (networkId: number) => {
            let ret: Promise<ProviderResult>;
            setLoading(true);
            try {
                let brt = await NetworkFactory.NetworkBusiness.getById(networkId);
                if (brt.sucesso) {
                    setLoading(false);
                    _setNetwork(brt.dataResult);
                    return {
                        ...ret,
                        sucesso: true,
                        mensagemSucesso: "Load Network"
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
        getBySlug: async (networkSlug: string) => {
            let ret: Promise<NetworkProviderResult>;
            setLoading(true);
            _setNetwork(null);
            try {
                let brt = await NetworkFactory.NetworkBusiness.getBySlug(networkSlug);
                if (brt.sucesso) {
                    setLoading(false);
                    _setNetwork(brt.dataResult);
                    return {
                        ...ret,
                        network: brt.dataResult,
                        sucesso: true,
                        mensagemSucesso: "Load Network"
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
        getUserNetwork: async (networkId: number) => {
            let ret: Promise<ProviderResult>;
            setLoading(true);
            try {
                let brt = await NetworkFactory.NetworkBusiness.getUserNetwork(networkId);
                if (brt.sucesso) {
                    setLoading(false);
                    _setUserNetwork(brt.dataResult);
                    //_setNetwork(brt.dataResult.network);
                    if (brt.dataResult) {
                        _setCurrentRole(brt.dataResult.role);
                    }
                    return {
                        ...ret,
                        sucesso: true,
                        mensagemSucesso: "Load Network"
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
        getUserNetworkBySlug: async (networkSlug: string) => {
            let ret: Promise<ProviderResult>;
            setLoading(true);
            try {
                let brt = await NetworkFactory.NetworkBusiness.getUserNetworkBySlug(networkSlug);
                if (brt.sucesso) {
                    setLoading(false);
                    _setUserNetwork(brt.dataResult);
                    if (brt.dataResult) {
                        _setCurrentRole(brt.dataResult.role);
                    }
                    return {
                        ...ret,
                        sucesso: true,
                        mensagemSucesso: "Load Network"
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
        getSellerBySlug: async (networkSlug: string, userSlug: string) => {
            let ret: Promise<ProviderResult>;
            setLoadingSeller(true);
            try {
                let brt = await NetworkFactory.NetworkBusiness.getSellerBySlug(networkSlug, userSlug);
                if (brt.sucesso) {
                    setLoadingSeller(false);
                    setSeller(brt.dataResult);
                    return {
                        ...ret,
                        sucesso: true,
                        mensagemSucesso: "Load Network"
                    };
                }
                else {
                    setLoadingSeller(false);
                    return {
                        ...ret,
                        sucesso: false,
                        mensagemErro: brt.mensagem
                    };
                }
            }
            catch (err) {
                setLoadingSeller(false);
                return {
                    ...ret,
                    sucesso: false,
                    mensagemErro: JSON.stringify(err)
                };
            }
        },
        requestAccess: async (networkId: number, referrerId?: number) => {
            let ret: Promise<ProviderResult>;
            setLoadingRequestAccess(true);
            try {
                let brt = await NetworkFactory.NetworkBusiness.requestAccess(networkId, referrerId);
                if (brt.sucesso) {
                    setLoadingRequestAccess(false);
                    return {
                        ...ret,
                        sucesso: true,
                        mensagemSucesso: "Network list"
                    };
                }
                else {
                    setLoadingRequestAccess(false);
                    return {
                        ...ret,
                        sucesso: false,
                        mensagemErro: brt.mensagem
                    };
                }
            }
            catch (err) {
                setLoadingRequestAccess(false);
                return {
                    ...ret,
                    sucesso: false,
                    mensagemErro: JSON.stringify(err)
                };
            }
        },
        changeStatus: async (networkId: number, userId: number, status: number) => {
            let ret: Promise<ProviderResult>;
            setLoadingUpdate(true);
            try {
                let brt = await NetworkFactory.NetworkBusiness.changeStatus(networkId, userId, status);
                if (brt.sucesso) {
                    setLoadingUpdate(false);
                    return {
                        ...ret,
                        sucesso: true,
                        mensagemSucesso: "Network list"
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
        promote: async (networkId: number, userId: number) => {
            let ret: Promise<ProviderResult>;
            setLoadingUpdate(true);
            try {
                let brt = await NetworkFactory.NetworkBusiness.promote(networkId, userId);
                if (brt.sucesso) {
                    setLoadingUpdate(false);
                    return {
                        ...ret,
                        sucesso: true,
                        mensagemSucesso: "Network list"
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
        demote: async (networkId: number, userId: number) => {
            let ret: Promise<ProviderResult>;
            setLoadingUpdate(true);
            try {
                let brt = await NetworkFactory.NetworkBusiness.demote(networkId, userId);
                if (brt.sucesso) {
                    setLoadingUpdate(false);
                    return {
                        ...ret,
                        sucesso: true,
                        mensagemSucesso: "Network list"
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
        }
    }

    return (
        <NetworkContext.Provider value={networkProviderValue}>
            {props.children}
        </NetworkContext.Provider>
    );
}