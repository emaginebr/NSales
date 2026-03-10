import { useNavigate, useParams } from "react-router-dom";
import { useContext, useEffect } from "react";
import AuthContext from "../../Contexts/Auth/AuthContext";
import Skeleton from "react-loading-skeleton";
import NetworkInfo from "../../DTO/Domain/NetworkInfo";
import EditMode from "../../Components/EditMode";
import { StringDictionary } from "../../Components/StringDictionary";

interface IHeroParam {
    loading: boolean,
    isEditing: boolean,
    variables: StringDictionary
};

export default function Hero01Part(param: IHeroParam) {

    let navigate = useNavigate();

    const authContext = useContext(AuthContext);

    let { networkSlug } = useParams();

    return (
        <>
            <div className="container-fluid px-4 py-5 my-5 text-center">
                <div className="lc-block mb-4">
                    <div>
                        <h2 className="display-2 fw-bold">
                            {param.loading ?
                                <Skeleton />
                                :
                                <>
                                    <EditMode.Text 
                                        name="HERO_TITLE" 
                                        value={param.variables["HERO_TITLE"]} 
                                        isEditing={param.isEditing} 
                                    />
                                </>
                            }

                        </h2>
                    </div>
                </div>
                <div className="lc-block col-lg-6 mx-auto mb-5">
                    <div>
                        <p className="lead">
                            {param.loading ?
                                <Skeleton />
                                :
                                <>
                                    <EditMode.Text 
                                        name="HERO_SLOGAN" 
                                        value={param.variables["HERO_SLOGAN"]} 
                                        isEditing={param.isEditing} 
                                    />
                                </>
                            }
                        </p>
                    </div>
                </div>

                <div className="lc-block d-grid gap-2 d-sm-flex justify-content-sm-center mb-5">
                    <EditMode.Btn
                        name="HERO_LINK_TO_PLANS"
                        value={param.variables["HERO_LINK_TO_PLANS"]} 
                        className="btn btn-primary btn-lg px-4 gap-3"
                        isEditing={param.isEditing}
                        href="#plans"
                    />
                    <EditMode.Btn
                        name="HERO_BECOME_A_SELLER"
                        value={param.variables["HERO_BECOME_A_SELLER"]} 
                        variant="outline-secondary"
                        size="lg"
                        className="px-4"
                        isEditing={param.isEditing}
                        onClick={(e) => {
                            e.preventDefault();
                            if (authContext.sessionInfo) {
                                navigate("/" + networkSlug + "/request-access");
                            }
                            else {
                                navigate("/" + networkSlug + "/new-seller");
                            }
                        }}
                    />
                </div>
                <div className="lc-block d-grid gap-2 d-sm-flex justify-content-sm-center">
                    <EditMode.Img
                        name="HERO_IMAGE"
                        className="img-fluid"
                        defaultSrc="https://emagine.nyc3.digitaloceanspaces.com/monexup/fixed/hero01.svg"
                        style={{width: "auto", height: "783px"}}
                        isEditing={param.isEditing}
                    />
                </div>
            </div>
        </>
    );
}