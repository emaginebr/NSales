import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { useNavigate } from "react-router-dom";
import Container from "react-bootstrap/esm/Container";
import Row from "react-bootstrap/esm/Row";
import Col from "react-bootstrap/esm/Col";
import { faBoltLightning, faLock, faFileUpload, faCalendarAlt, faFileWord, faBoxOpen, faLockOpen, faUserDoctor } from '@fortawesome/free-solid-svg-icons';
import { faBitcoin } from "@fortawesome/free-brands-svg-icons";
import { useTranslation } from "react-i18next";

export default function Features() { // Renamed component to Features to match filename, assuming Header was a typo

    const { t } = useTranslation();
    let navigate = useNavigate();

    return (
        <>
            <section id="overview" className="py-5">
                <Container>
                    <Row>
                        <Col md={12} className="text-center">
                            <h4 className="display-2 mb-0">{t('home_features_title')}</h4>
                            <p>{t('home_features_subtitle')}</p>
                        </Col>
                    </Row>

                    <Row className="pt-4">
                        <Col md={4}>
                            <div className="lc-block border p-4" style={{minHeight: "263px"}}>
                                <div className="text-center">
                                    <FontAwesomeIcon icon={faBitcoin} fixedWidth size="4x" />
                                    <h4 className="my-3" dangerouslySetInnerHTML={{ __html: t('home_feature_product_catalog_title') }} />
                                    <p>{t('home_feature_product_catalog_desc')}</p>
                                </div>
                            </div>
                        </Col>
                        <Col md={4}>
                            <div className="lc-block border p-4" style={{minHeight: "263px"}}>
                                <div className="text-center">
                                    <FontAwesomeIcon icon={faLock} fixedWidth size="4x" />
                                    <h4 className="my-3" dangerouslySetInnerHTML={{ __html: t('home_feature_multiple_networks_title') }} />
                                    <p>{t('home_feature_multiple_networks_desc')}</p>
                                </div>
                            </div>
                        </Col>
                        <Col md={4}>
                            <div className="lc-block border p-4" style={{minHeight: "263px"}}>
                                <div className="text-center">
                                    <FontAwesomeIcon icon={faBoltLightning} fixedWidth size="4x" />
                                    <h4 className="my-3" dangerouslySetInnerHTML={{ __html: t('home_feature_products_services_title') }} />
                                    <p>{t('home_feature_products_services_desc')}</p>
                                </div>
                            </div>
                        </Col>
                    </Row>
                    <Row className="pt-4">
                        <Col md={4}>
                            <div className="lc-block border p-4" style={{minHeight: "263px"}}>
                                <div className="text-center">
                                    <FontAwesomeIcon icon={faFileUpload} fixedWidth size="4x" />
                                    <h4 className="my-3" dangerouslySetInnerHTML={{ __html: t('home_feature_team_management_title') }} />
                                    <p>{t('home_feature_team_management_desc')}</p>
                                </div>
                            </div>
                        </Col>
                        <Col md={4}>
                            <div className="lc-block border p-4" style={{minHeight: "263px"}}>
                                <div className="text-center">
                                    <FontAwesomeIcon icon={faCalendarAlt} fixedWidth size="4x" />
                                    <h4 className="my-3">{t('home_feature_admin_panel_title')}</h4>
                                    <p>{t('home_feature_admin_panel_desc')}</p>
                                </div>
                            </div>
                        </Col>
                        <Col md={4}>
                            <div className="lc-block border p-4" style={{minHeight: "263px"}}>
                                <div className="text-center">
                                    <FontAwesomeIcon icon={faFileWord} fixedWidth size="4x" />
                                    <h4 className="my-3">{t('home_feature_reports_commissions_title')}</h4>
                                    <p>{t('home_feature_reports_commissions_desc')}</p>
                                </div>
                            </div>
                        </Col>
                    </Row>
                    <Row className="pt-4">
                        <Col md={4}>
                            <div className="lc-block border p-4" style={{minHeight: "263px"}}>
                                <div className="text-center">
                                    <FontAwesomeIcon icon={faLockOpen} fixedWidth size="4x" />
                                    <h4 className="my-3">{t('home_feature_responsive_interface_title')}</h4>
                                    <p>{t('home_feature_responsive_interface_desc')}</p>
                                </div>
                            </div>
                        </Col>
                        <Col md={4}>
                            <div className="lc-block border p-4" style={{minHeight: "263px"}}>
                                <div className="text-center">
                                    <FontAwesomeIcon icon={faUserDoctor} fixedWidth size="4x" />
                                    <h4 className="my-3">{t('home_feature_interactive_network_tree_title')}</h4>
                                    <p>{t('home_feature_interactive_network_tree_desc')}</p>
                                </div>
                            </div>
                        </Col>
                        <Col md={4}>
                            <div className="lc-block border p-4" style={{minHeight: "263px"}}>
                                <div className="text-center">
                                    <FontAwesomeIcon icon={faBoxOpen} fixedWidth size="4x" />
                                    <h4 className="my-3">{t('home_feature_security_transparency_title')}</h4>
                                    <p>{t('home_feature_security_transparency_desc')}</p>
                                </div>
                            </div>
                        </Col>
                    </Row>
                </Container>
            </section>
        </>
    );
}