import { faSave } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React from "react";

export const CustomToolbar = (onSave: any) => (
    <div id="custom-toolbar">
      <select className="ql-header" defaultValue="">
        <option value="1" />
        <option value="2" />
        <option value="" />
      </select>
      <select className="ql-size" defaultValue="">
        <option value="small" />
        <option value="normal" />
        <option value="large" />
        <option value="huge" />
      </select>
      <button className="ql-bold" />
      <button className="ql-italic" />
      <button className="ql-underline" />
      <button className="ql-link" />
      <button className="ql-clean" />
  
      {/* Botão de salvar fora da API do Quill, então chamamos direto */}
      <button
        type="button"
        className="ql-save"
        onClick={(e) => {
          e.preventDefault();
          if (onSave) onSave();
        }}
      >
        <FontAwesomeIcon icon={faSave} fixedWidth />
      </button>
    </div>
  );