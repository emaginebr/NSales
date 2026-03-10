import { useContext, useEffect, useState } from "react";
import AuthContext from "../../Contexts/Auth/AuthContext";
import { Link, useNavigate } from "react-router-dom";
import Header from "./Header";
import Footer from "./Footer";
import Features from "./Features";
import Pricing from "./Pricing";
import NetworkPart from "./NetworkPart";
import UserPart from "./UserPart";
import UserContext from "../../Contexts/User/UserContext";
import NetworkContext from "../../Contexts/Network/NetworkContext";



export default function HomePage() {

    const authContext = useContext(AuthContext);
    const userContext = useContext(UserContext);
    const networkContext = useContext(NetworkContext);

    let navigate = useNavigate();

    useEffect(() => {
        userContext.list(3).then((ret) => {
            if (!ret.sucesso) {
                alert(ret.mensagemErro);
            }
        });
        networkContext.listAll().then((ret) => {
            if (!ret.sucesso) {
                alert(ret.mensagemErro);
            }
        });
    }, []);

    return (
        <>
            <Header />
            <Features />
            <NetworkPart 
                loading={networkContext.loading} 
                networks={networkContext.networks} 
            />
            <Pricing />
            <UserPart 
                loading={userContext.loadingList}
                users={userContext.users} 
            />
            <Footer />
        </>
    );

}