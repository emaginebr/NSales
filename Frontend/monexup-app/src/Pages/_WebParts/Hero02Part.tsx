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

export default function Hero02Part(param: IHeroParam) {

    let navigate = useNavigate();

    const authContext = useContext(AuthContext);

    let { networkSlug } = useParams();

    return (
        <>
            <div className="overflow-hidden">
                <div className="container-fluid col-xxl-8">
                    <div className="row flex-lg-nowrap align-items-center g-5">
                        <div className="order-lg-1 w-100">
                            <EditMode.Img
                                name="HERO_IMAGE"
                                className="d-block mx-lg-auto img-fluid"
                                defaultSrc="https://images.unsplash.com/photo-1618004912476-29818d81ae2e?crop=entropy&amp;cs=tinysrgb&amp;fit=crop&amp;fm=jpg&amp;ixid=MnwzNzg0fDB8MXxzZWFyY2h8NzV8fHB1cnBsZXxlbnwwfDB8fHwxNjQ3NDcxNjY4&amp;ixlib=rb-1.2.1&amp;q=80&amp;w=1080&amp;h=768"
                                style={{ width: "2160px", height: "768px", clipPath: "polygon(25% 0%, 100% 0%, 100% 99%, 0% 100%)" }}
                                isEditing={param.isEditing}
                            />
                        </div>
                        <div className="col-lg-6 col-xl-5 text-center text-lg-start pt-lg-5 mt-xl-4">
                            <div className="lc-block mb-4">
                                <div>
                                    <h1 className="fw-bold display-3">
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
                                    </h1>
                                </div>
                            </div>

                            <div className="lc-block mb-5">
                                <div>
                                    <p className="rfs-8">
                                        {param.loading ?
                                            <Skeleton />
                                            :
                                            <>
                                                <EditMode.Text
                                                    name="HERO_RESUME"
                                                    value={param.variables["HERO_RESUME"]}
                                                    isEditing={param.isEditing}
                                                />
                                            </>
                                        }
                                    </p>
                                </div>
                            </div>

                            <div className="lc-block mb-5">
                                <EditMode.Btn
                                    name="HERO_BECOME_A_SELLER"
                                    value={param.variables["HERO_BECOME_A_SELLER"]}
                                    variant="primary"
                                    size="lg"
                                    className="px-4 me-md-2 btn-lg"
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

                            <div className="lc-block">
                                <div>
                                    <p className="fw-bold">
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
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
}