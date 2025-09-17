import Card from "react-bootstrap/esm/Card";
import CardBody from "react-bootstrap/esm/CardBody";
import Col from "react-bootstrap/esm/Col";
import Container from "react-bootstrap/esm/Container";
import Row from "react-bootstrap/esm/Row";
import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";

export default function Error404Page() {

    const { t } = useTranslation();

    return (

        <div className="page-wrap d-flex flex-row align-items-center py-5">
            <Container>
                <Row className="justify-content-center">
                    <Col md={6} className="text-center">
                        <Card>
                            <CardBody>
                                <span className="display-1 d-block">404</span>
                                <div className="mb-4 lead">{t('error404_message')}</div>
                                <Link to="/" className="btn btn-link">{t('error404_back_to_home')}</Link>
                            </CardBody>
                        </Card>
                    </Col>
                </Row>
            </Container>
        </div>
    );
}