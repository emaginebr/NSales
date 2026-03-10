import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import Col from "react-bootstrap/esm/Col";
import Container from "react-bootstrap/esm/Container";
import Row from "react-bootstrap/esm/Row";
import { faEnvelope, faUserCircle } from '@fortawesome/free-solid-svg-icons';
import { faWhatsapp } from '@fortawesome/free-brands-svg-icons';
import { Link, useNavigate, useParams } from "react-router-dom";
import { useContext, useEffect } from "react";
import NetworkContext from "../../Contexts/Network/NetworkContext";
import Skeleton from "react-loading-skeleton";
import { UserRoleEnum } from "../../DTO/Enum/UserRoleEnum";
import UserNetworkInfo from "../../DTO/Domain/UserNetworkInfo";
import UserAddressInfo from "../../DTO/Domain/UserAddressInfo";
import { showProfile } from "../../Components/Functions";
import { StringDictionary } from "../../Components/StringDictionary";
import EditMode from "../../Components/EditMode";
import { useTranslation } from "react-i18next";

interface ITeamParam {
    loading: boolean,
    teams?: UserNetworkInfo[],
    isEditing: boolean,
    variables: StringDictionary
};

export default function TeamPart(param: ITeamParam) {

    let { networkSlug } = useParams();

    const { t } = useTranslation();

    const showDescription = (user: UserNetworkInfo) => {
        if (user.user?.addresses && user.user?.addresses.length > 0) {
            let address: UserAddressInfo = user.user.addresses[0];
            return address.city + "/" + address.state;
        }
        return "";
    };

    return (
        <>
            <Container className="mb-3">
                <Row>
                    <Col md={12} className="text-center">
                        <div className="lc-block mb-1">
                            <h2 className="display-2 mb-0">
                                <b>{param.loading ?
                                    <Skeleton />
                                    :
                                    <>
                                        <EditMode.Text
                                            name="TEAM_TITLE"
                                            value={param.variables["TEAM_TITLE"]}
                                            isEditing={param.isEditing}
                                        />
                                    </>
                                }</b>
                            </h2>
                            <p>{param.loading ?
                                <Skeleton />
                                :
                                <>
                                    <EditMode.Text
                                        name="TEAM_DESCRIPTION"
                                        value={param.variables["TEAM_DESCRIPTION"]}
                                        isEditing={param.isEditing}
                                    />
                                </>
                            }</p>
                        </div>
                    </Col>
                </Row>
                <Row className="pt-4">
                    {param.loading ?
                        <>
                            {[1, 2, 3, 4]?.map((index) => {
                                return (
                                    <Col md={3} className="text-center py-4">
                                        <div className="lc-block">
                                            <Skeleton circle={true} className="mb-3" style={{ width: "10rem", height: "10rem" }} />
                                            <h5><Skeleton /></h5>
                                            <small className="text-secondary"><Skeleton /></small>
                                        </div>
                                        <div className="lc-block mt-2 border-top">
                                            <a className="text-dark text-decoration-none" href="#">
                                                <FontAwesomeIcon icon={faEnvelope} />
                                            </a>
                                            &nbsp;
                                            <a className="text-dark text-decoration-none" href="#">
                                                <FontAwesomeIcon icon={faWhatsapp} />
                                            </a>
                                        </div>
                                    </Col>
                                );
                            })}
                        </>
                        :
                        <>
                            {param.teams?.map((user) => {
                                return (
                                    <Col md={3} className="text-center py-4">
                                        <div className="lc-block">
                                            {user.user?.imageUrl ?
                                                <Link to={"/" + networkSlug + "/@/" + user.user?.slug}>
                                                    <img src={user.user?.imageUrl} className="rounded-circle mb-3" style={{ width: "10rem", height: "10rem" }} />
                                                </Link>
                                                :
                                                <Link to={"/" + networkSlug + "/@/" + user.user?.slug}>
                                                    <FontAwesomeIcon icon={faUserCircle} size="8x" className="rounded-circle mb-3" style={{ height: "10rem" }} />
                                                </Link>
                                            }
                                            <h5>
                                                <Link to={"/" + networkSlug + "/@/" + user.user?.slug}>
                                                    <strong>{user.user?.name}</strong>
                                                </Link>
                                            </h5>
                                            <small className="text-secondary" style={{ letterSpacing: "1px" }}>{showProfile(user, t)}</small>
                                        </div>
                                        <div className="lc-block mt-2 border-top">
                                            <a className="text-dark text-decoration-none" href="#">
                                                <FontAwesomeIcon icon={faEnvelope} />
                                            </a>
                                            &nbsp;
                                            <a className="text-dark text-decoration-none" href="#">
                                                <FontAwesomeIcon icon={faWhatsapp} />
                                            </a>
                                        </div>
                                    </Col>
                                );
                            })}
                        </>
                    }
                </Row>
            </Container>
        </>
    );
}