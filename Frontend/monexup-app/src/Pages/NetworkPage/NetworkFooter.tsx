import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import Col from "react-bootstrap/esm/Col";
import Container from "react-bootstrap/esm/Container";
import Row from "react-bootstrap/esm/Row";
import { faFacebook, faInstagram, faTwitter } from '@fortawesome/free-brands-svg-icons';
import { Link, useNavigate, useParams } from "react-router-dom";
import { useContext } from "react";
import AuthContext from "../../Contexts/Auth/AuthContext";
import { useTranslation } from "react-i18next";

export default function Footer() {

    let { networkSlug } = useParams();

    const { t } = useTranslation();

    const authContext = useContext(AuthContext);

    return (
        <footer className="bg-dark text-light">
            <Container className="py-5">
                <Row>
                    <Col md={3}>
                        <div className="lc-block small">
                            <p>{t('footer_description')}</p>
                        </div>
                        <div className="lc-block py-2">
                            <a className="text-decoration-none me-3" href="#" aria-label={t('footer_social_facebook')}>
                                <FontAwesomeIcon icon={faFacebook} size="2x" fixedWidth />
                            </a>
                            <a className="text-decoration-none me-3" href="#" aria-label={t('footer_social_twitter')}>
                                <FontAwesomeIcon icon={faTwitter} size="2x" fixedWidth />
                            </a>
                            <a className="text-decoration-none" href="#" aria-label={t('footer_social_instagram')}>
                                <FontAwesomeIcon icon={faInstagram} size="2x" fixedWidth />
                            </a>
                        </div>
                    </Col>

                    <Col md={2} className="offset-md-1">
                        <div className="lc-block small">
                            <p><Link to={"/" + networkSlug}>{t("home")}</Link></p>
                            <p><a href="#plans">{t('footer_plans')}</a></p>
                        </div>
                    </Col>

                    <Col md={2} className="offset-md-1">
                        <div className="lc-block small">
                            <p><Link to={"/network"}>{t("create_your_network")}</Link></p>
                            <p>{authContext.sessionInfo ?
                                <Link to={"/" + networkSlug + "/request-access"}>{t("be_a_representative")}</Link> 
                                :
                                <Link to={"/" + networkSlug + "/new-seller"}>{t("be_a_representative")}</Link>
                            }</p>
                        </div>
                    </Col>

                    <Col md={2} className="offset-md-1">
                        <div className="lc-block small">
                            <p><Link to={"/" + networkSlug + "/account/login?returnUrl=%2Fadmin%2Fdashboard"}>{t('footer_dashboard')}</Link></p>
                        </div>
                    </Col>
                </Row>
            </Container>

            <Container className="py-4 border-top border-secondary">
                <Row>
                    <Col md={6} className="small">
                        <p className="mb-0">{t('footer_copyright_current_year', { year: new Date().getFullYear() })}</p>
                    </Col>
                    <Col md={6} className="text-end small">
                        <p className="mb-0">{t('footer_all_rights_reserved')}</p>
                    </Col>
                </Row>
            </Container>
        </footer>
    );
}
