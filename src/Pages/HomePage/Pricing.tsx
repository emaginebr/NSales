import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { useNavigate } from "react-router-dom";
import Container from "react-bootstrap/esm/Container";
import Row from "react-bootstrap/esm/Row";
import Col from "react-bootstrap/esm/Col";
import { faBoltLightning, faLock, faFileUpload, faCalendarAlt, faFileWord, faBoxOpen, faLockOpen, faUserDoctor } from '@fortawesome/free-solid-svg-icons';
import { faBitcoin } from "@fortawesome/free-brands-svg-icons";
import Card from "react-bootstrap/esm/Card";
import CardHeader from "react-bootstrap/esm/CardHeader";
import CardTitle from "react-bootstrap/esm/CardTitle";
import CardBody from "react-bootstrap/esm/CardBody";
import CardText from "react-bootstrap/esm/CardText";
import Button from "react-bootstrap/esm/Button";
import { useTranslation } from "react-i18next";

export default function Pricing() { // Renomeado de Header para Pricing

    const { t } = useTranslation();
    let navigate = useNavigate();

    return (
        <>
            <section id="plans" className="py-5">
                <Container className="pb-5">
                    <Row>
                        <Col md={12} className="text-center">
                            <div className="lc-block mb-4">
                                <h2 className="display-2 mb-0"><b>{t('home_pricing_title')}</b></h2>
                            </div>
                        </Col>
                    </Row>
                    <Row md={4} className="text-center">
                        <Col lg={4} md={6} className="text-dark my-2">
                            <Card>
                                <CardHeader className="py-3">
                                    <h4 className="my-0 fw-normal">{t('home_pricing_free_plan')}</h4>
                                </CardHeader>
                                <CardBody>
                                    <CardTitle>
                                        <span className="display-4"><b>{t('home_pricing_free_price')}</b></span>
                                        <span className="lead">{t('home_pricing_per_month')}</span>
                                    </CardTitle>
                                    <CardText className="my-4 lc-block">
                                        <div>
                                            <ul className="list-unstyled">
                                                <li>{t('home_pricing_free_feature1')}</li>
                                                <li>{t('home_pricing_free_feature2')}</li>
                                                <li>{t('home_pricing_free_feature3')}</li>
                                                <li>{t('home_pricing_free_feature4')}</li>
                                            </ul>
                                        </div>
                                    </CardText>
                                    <div className="d-grid lc-block">
                                        <Button variant="primary" size="lg" className="btn-outline-primary" onClick={(e) => {
                                            navigate("/network");
                                        }}>{t('home_pricing_free_button')}</Button>
                                    </div>
                                </CardBody>
                            </Card>
                        </Col>
                        <Col lg={4} md={6} className="text-dark my-2">
                            <Card>
                                <CardHeader className="py-3">
                                    <h4 className="my-0 fw-normal">{t('home_pricing_pro_plan')}</h4>
                                </CardHeader>
                                <CardBody>
                                    <CardTitle>
                                        <span className="display-4"><b>{t('home_pricing_pro_price')}</b></span>
                                        <span className="lead">{t('home_pricing_per_month')}</span>
                                    </CardTitle>
                                    <CardText className="my-4 lc-block">
                                        <div>
                                            <ul className="list-unstyled">
                                                <li>{t('home_pricing_pro_feature1')}</li>
                                                <li>{t('home_pricing_pro_feature2')}</li>
                                                <li>{t('home_pricing_pro_feature3')}</li>
                                                <li>{t('home_pricing_pro_feature4')}</li>
                                            </ul>
                                        </div>
                                    </CardText>
                                    <div className="d-grid lc-block">
                                        <Button variant="primary" size="lg" onClick={(e) => {
                                            navigate("/monexup/pro");
                                        }}>{t('home_pricing_coming_soon')}</Button>
                                    </div>
                                </CardBody>
                            </Card>
                        </Col>
                        <div className="col-lg-4 col-md-6 text-dark my-2">
                            <div className="card">
                                <div className="card-header py-3">
                                    <h4 className="my-0 fw-normal">{t('home_pricing_enterprise_plan')}</h4>
                                </div>
                                <div className="card-body">
                                    <h5 className="card-title">
                                        <span className="display-4"><b>{t('home_pricing_enterprise_price')}</b></span>
                                        <span className="lead">{t('home_pricing_per_month')}</span>
                                    </h5>

                                    <div className="card-text my-4 lc-block">
                                        <div>
                                            <ul className="list-unstyled">
                                                <li>{t('home_pricing_enterprise_feature1')}</li>
                                                <li>{t('home_pricing_enterprise_feature2')}</li>
                                                <li>{t('home_pricing_enterprise_feature3')}</li>
                                                <li>{t('home_pricing_enterprise_feature4')}</li>
                                            </ul>
                                        </div>
                                    </div>
                                    <div className="d-grid lc-block">
                                        <Button variant="primary" size="lg" onClick={(e) => {
                                            navigate("/monexup/pro");
                                        }}>{t('home_pricing_coming_soon')}</Button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </Row>
                </Container>
            </section>
        </>
    );
}