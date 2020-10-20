import React, { useState } from "react";
import { connect } from "react-redux";
import { reduxForm } from "redux-form";
import { createField, Input } from "../common/formControls/FormControls";
import { requiredField } from "../common/validators/Validators";
import s from "./RadioButtons.module.css";

let CreateRadioForm = ({ handleSubmit, error, uV, cV }) => {
  return (
    <form onSubmit={handleSubmit} className={s.radioFormFields}>
      {uV.map((d) => createField(cV.placeholder, cV.name, cV.component, cV.validators, { type: cV.type, value: d.value }, d.text))}
      {createField(null, "rememberMe", Input, [], { type: "checkbox" }, "Books Pair selected (2 files)")}
      {error && <div className={s.formSummaryError}>{error}</div>}
      <div>
        <button>Upload</button>
      </div>
    </form>
  );
};

const RadioReduxForm = reduxForm({ form: "radio" })(CreateRadioForm);

const RadioButtons = ({ uniqValues, commonValues }) => {
  const [radioResult, setRadioResult] = useState(0);

  const onSubmit = (formData) => {
    let f = formData.["files0"];
    debugger;
    setRadioResult(f);
  };

  return (
    <div>
      <div className={s.radioButtonResult}>Selected Language ={" " + radioResult}</div>
      <RadioReduxForm onSubmit={onSubmit} uV={uniqValues} cV={commonValues} />
    </div>
  );
};

export default RadioButtons;
