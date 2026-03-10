import Toast from 'react-bootstrap/Toast';
import ToastContainer from 'react-bootstrap/ToastContainer';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faInfo, faQuestion, faQuestionCircle, faWarning } from '@fortawesome/free-solid-svg-icons';
import { useState } from 'react';
import { MessageToastEnum } from '../DTO/Enum/MessageToastEnum';
import Button from 'react-bootstrap/esm/Button';

interface IMessageToastParam {
  dialog: MessageToastEnum;
  showMessage: boolean;
  messageText: string;
  onClose: () => void;
  onYes?: () => void;
  onNo?: () => void;
}

const showTitle = (dialog: MessageToastEnum) => {
  switch (dialog) {
    case MessageToastEnum.Error:
      return (
        <>
          <FontAwesomeIcon className='text-danger' icon={faWarning} />&nbsp;
          <strong className="me-auto">Error</strong>
        </>
      );
      break;
    case MessageToastEnum.Success:
      return (
        <>
          <FontAwesomeIcon className='text-success' icon={faWarning} />&nbsp;
          <strong className="me-auto">Success</strong>
        </>
      );
      break;
    case MessageToastEnum.Information:
      return (
        <>
          <FontAwesomeIcon className='text-info' icon={faInfo} />&nbsp;
          <strong className="me-auto">Information</strong>
        </>
      );
      break;
    case MessageToastEnum.Confirmation:
      return (
        <>
          <FontAwesomeIcon className='text-info' icon={faQuestionCircle} />&nbsp;
          <strong className="me-auto">Confirmation</strong>
        </>
      );
      break;
  }
};

export default function MessageToast(param: IMessageToastParam) {

  return (
    <ToastContainer
      className="p-3"
      position="bottom-end"
      style={{ zIndex: 9999 }}
    >
      <Toast animation={true} onClose={param.onClose} show={param.showMessage} delay={10000} autohide>
        <Toast.Header closeButton={true}>
          {showTitle(param.dialog)}
        </Toast.Header>
        <Toast.Body>
          {param.messageText}
          {param.dialog == MessageToastEnum.Confirmation &&
            <div className="mt-2 pt-2 border-top">
              <Button variant="success" size="sm" onClick={param.onYes}>Yes</Button>
              &nbsp;
              <Button variant="danger" size="sm" onClick={param.onNo}>No</Button>
            </div>
          }
        </Toast.Body>
      </Toast>
    </ToastContainer>
  );
}