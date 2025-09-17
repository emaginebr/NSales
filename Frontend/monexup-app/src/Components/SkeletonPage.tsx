import Col from "react-bootstrap/esm/Col";
import Container from "react-bootstrap/esm/Container";
import Row from "react-bootstrap/esm/Row";
import Skeleton from "react-loading-skeleton";

export default function SkeletonPage() {
    return (
        <Container className="py-5">
            <Row>
                <Col md={12}>
                    <h1 className="mb-5"><Skeleton /></h1>
                </Col>
            </Row>
            <Row>
                <Col md={9}>
                    <h3 className="mb-5"><Skeleton /></h3>
                    <div className="mb-5"><Skeleton count={3} /></div>
                    <div className="mb-5"><Skeleton count={5} /></div>
                </Col>
                <Col md={3}>
                    <h3><Skeleton circle={true} height={300} /></h3>
                    <p><Skeleton count={3} /></p>
                </Col>
            </Row>
            <Row>
                <Col md={3}>
                    <p><Skeleton count={2} /></p>
                    <h3><Skeleton circle={true} height={300} /></h3>
                </Col>
                <Col md={9}>
                    <h3 className="mb-5"><Skeleton /></h3>
                    <div className="mb-5"><Skeleton count={4} /></div>
                    <div className="mb-5"><Skeleton count={3} /></div>
                </Col>
            </Row>
        </Container>
    );
}