import React, { useContext, useEffect, useState } from 'react';
import Container from 'react-bootstrap/Container';
import Button from 'react-bootstrap/esm/Button';
import Nav from 'react-bootstrap/Nav';
import Navbar from 'react-bootstrap/Navbar';
import { Link, useNavigate, useParams } from 'react-router-dom';
import AuthContext from '../Contexts/Auth/AuthContext';
import NavDropdown from 'react-bootstrap/NavDropdown';
import Alert from 'react-bootstrap/Alert';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faWarning } from '@fortawesome/free-solid-svg-icons/faWarning'
import { faBitcoinSign, faBoltLightning, faBox, faBrazilianRealSign, faBuilding, faCancel, faCheck, faCheckCircle, faCircle, faCircleUser, faClose, faCog, faCoins, faDollar, faEthernet, faFileWord, faHome, faLock, faPencil, faSearch, faSignInAlt, faUser, faUserAlt, faUserCircle, faUserCog, faUserFriends, faUserGear, faUserGraduate, faUserGroup, faUserMd } from '@fortawesome/free-solid-svg-icons';
import MessageToast from './MessageToast';
import { MessageToastEnum } from '../DTO/Enum/MessageToastEnum';
import { UserRoleEnum } from '../DTO/Enum/UserRoleEnum';
import NetworkContext from '../Contexts/Network/NetworkContext';
import Skeleton from 'react-loading-skeleton';
import 'react-loading-skeleton/dist/skeleton.css';
import { MenuLanguage } from './Functions';
import { useTranslation } from 'react-i18next';
import TemplateContext from '../Contexts/Template/TemplateContext';


export default function MenuNetwork() {

  const [showAlert, setShowAlert] = useState<boolean>(true);

  const [showMessage, setShowMessage] = useState<boolean>(false);
  const [messageText, setMessageText] = useState<string>("");

  const { t } = useTranslation();

  const throwError = (message: string) => {
    setMessageText(message);
    setShowMessage(true);
  };

  let navigate = useNavigate();

  let { networkSlug, sellerSlug } = useParams();

  const authContext = useContext(AuthContext);
  const networkContext = useContext(NetworkContext);
  const templateContext = useContext(TemplateContext);

  useEffect(() => {
    authContext.loadUserSession().then((authRet) => {
      if (authRet.sucesso) {
        /*
        networkContext.listByUser().then((ret) => {
          if (!ret.sucesso) {
            throwError(ret.mensagemErro);
          }
        });
        */
      }
    });
  }, []);
  return (
    <>
      <MessageToast
        dialog={MessageToastEnum.Error}
        showMessage={showMessage}
        messageText={messageText}
        onClose={() => setShowMessage(false)}
      ></MessageToast>
      <Navbar expand="lg" className="navbar-dark bg-dark">
        <Container>
          <Link className='navbar-brand' to={"/" + networkSlug}>
            {networkContext.loading ? <Skeleton width={140} /> : networkContext.network?.name}
          </Link>
          <Navbar.Toggle aria-controls="basic-navbar-nav" />
          <Navbar.Collapse id="basic-navbar-nav">
            <Nav className="me-auto">
              <Link className='nav-link' to={"/" + networkSlug}><FontAwesomeIcon icon={faHome} fixedWidth /> {t('home')}</Link>
              <Link className='nav-link' to={
                authContext.sessionInfo ?
                  sellerSlug ?
                    "/" + networkSlug + "/@/" + sellerSlug + "/request-access"
                    :
                    "/" + networkSlug + "/request-access"
                  :
                  sellerSlug ?
                    "/" + networkSlug + "/@/" + sellerSlug + "/new-seller"
                    :
                    "/" + networkSlug + "/new-seller" // Apply t() to link texts
              }><FontAwesomeIcon icon={faUser} fixedWidth /> {t('be_a_representative')}</Link>
            </Nav>
          </Navbar.Collapse>
          <Navbar.Collapse>

            <Nav className="ms-auto justify-content-end">
              {authContext.sessionInfo && networkContext.currentRole >= UserRoleEnum.NetworkManager &&
                <>
                  <NavDropdown title={
                    <>
                      {templateContext.editMode ?
                        <><FontAwesomeIcon icon={faCheckCircle} />&nbsp;{t('edit_mode_on')}</>
                        :
                        <><FontAwesomeIcon icon={faCircle} />&nbsp;{t('edit_mode_off')}</>
                      }
                    </>
                  } id="basic-nav-dropdown">
                    <NavDropdown.ItemText className='small'>{t('role_description')}</NavDropdown.ItemText>
                    <NavDropdown.Divider />
                    <NavDropdown.Item onClick={(e) => {
                      templateContext.setEditMode(true);
                    }}><FontAwesomeIcon icon={faCheckCircle} />&nbsp;{t('edit_mode_on')}</NavDropdown.Item>
                    <NavDropdown.Item onClick={(e) => {
                      templateContext.setEditMode(false);
                    }}><FontAwesomeIcon icon={faCircle} />&nbsp;{t('edit_mode_off')}</NavDropdown.Item>
                  </NavDropdown>
                </>
              }
              <MenuLanguage />
              {
                authContext.sessionInfo ?
                  <NavDropdown title={
                    <>
                      <FontAwesomeIcon icon={faCircleUser} />&nbsp;
                      <span>{authContext.sessionInfo.name}</span>
                    </>
                  } id="basic-nav-dropdown">
                    <NavDropdown.Item onClick={async () => {
                      navigate("/" + networkSlug + "/account/edit-account"); // Apply t() to texts
                    }}><FontAwesomeIcon icon={faPencil} fixedWidth /> {t('edit_account')}</NavDropdown.Item>
                    <NavDropdown.Item onClick={async () => {
                      navigate("/" + networkSlug + "/account/change-password"); // Apply t() to texts
                    }}><FontAwesomeIcon icon={faLock} fixedWidth /> {t('change_password')}</NavDropdown.Item>
                    <NavDropdown.Divider />
                    <NavDropdown.Item onClick={async () => {
                      let ret = authContext.logout();
                      if (!ret.sucesso) {
                        throwError(ret.mensagemErro);
                      }
                      navigate("/" + networkSlug); // Apply t() to texts
                    }}><FontAwesomeIcon icon={faClose} fixedWidth /> {t('logout')}</NavDropdown.Item>
                  </NavDropdown>
                  :
                  <>
                    <Nav.Item>
                      <Button variant="danger" onClick={async () => { // Apply t() to button text
                        navigate("/" + networkSlug + "/account/login");
                      }}>
                        <FontAwesomeIcon icon={faSignInAlt} fixedWidth /> {t('sign_in')}
                      </Button>
                    </Nav.Item>
                  </>
              }
            </Nav>
          </Navbar.Collapse>
        </Container>
      </Navbar>
      {showAlert &&
        <Container className="mt-3">
          <Alert key="danger" variant="danger" onClose={() => setShowAlert(false)} dismissible>
            <FontAwesomeIcon icon={faWarning} /> {t('trial_version_warning')}
          </Alert>
        </Container>
      }
    </>
  );
}
