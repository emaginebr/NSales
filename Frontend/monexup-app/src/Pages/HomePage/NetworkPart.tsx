import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { Link, useNavigate } from "react-router-dom";
import Container from "react-bootstrap/esm/Container";
import Row from "react-bootstrap/esm/Row";
import Col from "react-bootstrap/esm/Col";
import Card from "react-bootstrap/esm/Card";
import { faBuilding } from '@fortawesome/free-solid-svg-icons';
import { useContext, useEffect } from "react";
import NetworkContext from "../../Contexts/Network/NetworkContext";
import Skeleton from "react-loading-skeleton";
import { useTranslation } from "react-i18next";
import NetworkInfo from "../../DTO/Domain/NetworkInfo";

interface INetworkParam {
    loading: boolean,
    networks: NetworkInfo[]
}

export default function NetworkPart(param: INetworkParam) {

    const { t } = useTranslation();

    return (
        <>
            <section className="py-4 py-lg-6 bg-light">
                <Container>
                    <Row className="mb-4">
                        <Col md={12} className="text-center">
                            <h4 className="display-2 mb-0">{t('home_networkpart_title')}</h4>
                        </Col>
                    </Row>
                    {param.loading &&
                        <Row>
                        {[1,2,3,4].map((index) => {
                            return (
                                <Col lg={3}>
                                    <Card className="shadow-lg p-3">
                                        <Skeleton style={{width: "100%", height: "7rem"}} />
                                        <Card.Body>
                                            <div className="lc-block text-center mb-3">
                                                <div>
                                                    <h3><Skeleton /></h3>
                                                </div>
                                            </div>
                                            <div className="lc-block text-center">
                                                <Skeleton count={2} />
                                            </div>
                                        </Card.Body>
                                    </Card>
                                </Col>
                            );
                        })}
                        </Row>
                    }
                    <Row className="mb-5">
                        {!param.loading && param.networks?.map((network) => {
                            return (
                                <Col lg={3}>
                                    <Card className="shadow-lg p-3">
                                        {network.imageUrl &&
                                            <Link to={"/" + network.slug}>
                                            <Card.Img src={network.imageUrl} style={{ width: "100%", height: "7rem", objectFit: "cover" }} />
                                            </Link>
                                        }
                                        <Card.Body>
                                            <div className="lc-block text-center mb-3">
                                                <div>
                                                    <h3><Link to={"/" + network.slug}>{network.name}</Link></h3>
                                                </div>
                                            </div>
                                            <div className="lc-block text-center">
                                                {t('home_networkpart_affiliate_sellers', { count: network.qtdyUsers })}&nbsp;
                                                {t('home_networkpart_open_positions', { count: (network.maxUsers - network.qtdyUsers) })}
                                            </div>
                                        </Card.Body>
                                    </Card>
                                </Col>
                            );
                        })}
                    </Row>
                </Container>
            </section>
        </>
    );
}