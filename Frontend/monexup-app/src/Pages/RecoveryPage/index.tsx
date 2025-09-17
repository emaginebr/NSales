import { useContext } from "react";
import Col from "react-bootstrap/esm/Col";
import Container from "react-bootstrap/esm/Container";
import Row from "react-bootstrap/esm/Row";
import Form from 'react-bootstrap/Form';
import InputGroup from 'react-bootstrap/InputGroup';
import AuthContext from "../../Contexts/Auth/AuthContext";
import Button from "react-bootstrap/esm/Button";
import Card from 'react-bootstrap/Card';
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faBitcoinSign, faClose, faEnvelope, faLock, faMailBulk, faSave, faSign, faSignIn, faSignInAlt, faTrash, faUser } from '@fortawesome/free-solid-svg-icons';
import Table from "react-bootstrap/esm/Table";
import { Link } from "react-router-dom";

export default function RecoveryPage() {

    const authContext = useContext(AuthContext);

    return (
        <Container>
            <Row>
                <Col md="6" className='offset-md-3'>
                    <Card>
                        <Card.Header>
                            <h3 className="text-center">Recovery Password</h3>
                        </Card.Header>
                        <Card.Body>
                            <Form>
                                <div className="text-center mb-3">
                                    Registration is not required to make swaps, but you can do so anyway to access your transaction history.
                                </div>
                                <Form.Group as={Row} className="mb-3">
                                    <Form.Label column sm="2">Email:</Form.Label>
                                    <Col sm="10">
                                        <InputGroup>
                                            <InputGroup.Text><FontAwesomeIcon icon={faEnvelope} fixedWidth /></InputGroup.Text>
                                            <Form.Control type="email" placeholder="Your email" size="lg" onChange={(e) => {
                                                //_setName(e.target.value);
                                            }} />
                                        </InputGroup>
                                    </Col>
                                </Form.Group>
                                <div className="d-grid gap-2 d-md-flex justify-content-md-end">
                                    <Button variant="danger" size="lg"><FontAwesomeIcon icon={faEnvelope} fixedWidth /> Send recovery password email</Button>
                                </div>
                            </Form>
                        </Card.Body>
                    </Card>
                </Col>
            </Row>
        </Container>
    );
}