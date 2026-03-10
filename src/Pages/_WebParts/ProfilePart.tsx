import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faUserCircle } from '@fortawesome/free-solid-svg-icons';
import { faWhatsapp } from "@fortawesome/free-brands-svg-icons";
import { formatPhoneNumber, showProfile } from "../../Components/Functions";
import Skeleton from "react-loading-skeleton";
import UserInfo from "../../DTO/Domain/UserInfo";
import UserNetworkInfo from "../../DTO/Domain/UserNetworkInfo";
import EditMode from "../../Components/EditMode";
import { StringDictionary } from "../../Components/StringDictionary";
import { useTranslation } from "react-i18next";

interface IProfilePartParam {
    loading: boolean;
    user?: UserInfo;
    userNetwork?: UserNetworkInfo;
    isEditing: boolean,
    variables: StringDictionary
};

export default function ProfilePart(param: IProfilePartParam) {

  const { t } = useTranslation();

    return (
        <>
            <div className="position-relative">
                <div style={{ position: "absolute", top: "0px", width: "100%" }}>
                    <div className="container text-end">
                        <h1 className="mt-4 text-white" style={{ textShadow: "2px 2px 5px rgba(0, 0, 0, 0.7)" }}>
                            {param.loading ?
                                <Skeleton width={400} />
                                :
                                param.user?.phones &&
                                <>
                                    <FontAwesomeIcon icon={faWhatsapp} /> &nbsp;{formatPhoneNumber(param.user?.phones[0].phone)}
                                </>
                            }
                        </h1>
                    </div>
                </div>
                <div className="container-fluid g-0">
                    <img style={{ maxHeight: "400px", objectFit: "cover" }} className="img-fluid w-100 min-vh-25 min-vh-md-50 mb-n7" src="https://images.unsplash.com/photo-1457388497438-b12745cbc24f?crop=entropy&amp;cs=tinysrgb&amp;fit=crop&amp;fm=webp&amp;ixid=M3wzNzg0fDB8MXxzZWFyY2h8NjJ8fHdvbWFuJTIwc2l0dGluZyUyMG9uJTIwZ3JhcyUyMFdhbGtpbmclMjBhcm91bmR8ZW58MHwwfHx8MTcwMzA5MTYyNXww&amp;ixlib=rb-4.0.3&amp;q=80&amp;w=1080&amp;h=768" />
                </div>
                <div className="container position-relative" style={{ marginTop: "-15rem", zIndex: 500 }}>
                    <div className="row">
                        <div className="col col-md-8">
                            {param.loading ?
                                <Skeleton circle={true} style={{
                                    float: "left",
                                    marginRight: "1rem",
                                    borderWidth: "5px",
                                    borderColor: "#fff",
                                    borderStyle: "solid",
                                    width: "10rem",
                                    height: "10rem"
                                }} />
                                :
                                <>
                                    {param.user?.imageUrl ?
                                        <img src={param.user.imageUrl} className="rounded-circle mb-3" style={{
                                            float: "left",
                                            marginRight: "1rem",
                                            borderWidth: "5px",
                                            borderColor: "#fff",
                                            borderStyle: "solid",
                                            width: "10rem",
                                            height: "10rem"
                                        }} />
                                        :
                                        <FontAwesomeIcon icon={faUserCircle} size="10x" style={{
                                            float: "left",
                                            marginRight: "1rem",
                                            width: "10rem",
                                            height: "10rem",
                                            textShadow: "2px 2px 5px rgba(0, 0, 0, 0.7)"
                                        }} />
                                    }
                                </>
                            }
                            <h1 className="display-4 text-white" style={{
                                marginTop: "2.4rem",
                                textShadow: "2px 2px 5px rgba(0, 0, 0, 0.7)"
                            }}>{
                                    param.loading ?
                                        <Skeleton width={400} />
                                        :
                                        param.user?.name
                                }</h1>
                            {param.userNetwork &&
                                <h3 className="mt-2">{
                                    param.loading ?
                                        <Skeleton width={400} />
                                        :
                                        <>
                                            &nbsp;{showProfile(param.userNetwork, t)}
                                        </>
                                }</h3>
                            }
                        </div>
                    </div>
                </div>
            </div>
            <div className="container p-5 bg-body position-relative rounded" style={{ marginTop: "-4rem" }}>
                <div className="row">
                    <div className="col-md-4 text-center align-self-center">
                        <div className="lc-block border-lg-end border-2 ">
                            <div>
                                <p className="display-4 text-secondary">
                                    {param.loading ?
                                        <Skeleton />
                                        :
                                        <>
                                            <EditMode.Text
                                                name="PROFILE_SLOGAN"
                                                value={param.variables["PROFILE_SLOGAN"]}
                                                isEditing={param.isEditing}
                                            />
                                        </>
                                    }
                                </p>
                            </div>
                        </div>
                    </div>
                    <div className="col-md-8">
                        <div className="lc-block ">
                            <div>
                                <p className="display-4">
                                    {param.loading ?
                                        <Skeleton />
                                        :
                                        <>
                                            <EditMode.Text
                                                name="PROFILE_DESCRIPTION"
                                                value={param.variables["PROFILE_DESCRIPTION"]}
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
        </>
    );
}