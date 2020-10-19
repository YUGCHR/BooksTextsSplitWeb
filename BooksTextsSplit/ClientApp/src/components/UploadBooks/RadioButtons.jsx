import React, { useState } from "react";
import { connect } from "react-redux";
import { reduxForm } from "redux-form";
import { createField, Input } from "../common/formControls/FormControls";
import { requiredField } from "../common/validators/Validators";
import s from "./RadioButtons.module.css";

let CreateRadioForm = ({ handleSubmit, error, radioInitialValues }) => {
  return (
    <form onSubmit={handleSubmit} className={s.radioFormFields}>
      {radioInitialValues.map((data) => createField(null, "files", Input, [], { type: "radio", value: data.value }, data.text))}
      {createField(null, "rememberMe", Input, [], { type: "checkbox" }, "Books Pair selected (2 files)")}
      {error && <div className={s.formSummaryError}>{error}</div>}
      <div>
        <button>Upload</button>
      </div>
    </form>
  );
};

const RadioReduxForm = reduxForm({ form: "radio" })(CreateRadioForm);

const RadioButtons = ({radioInitialValues}) => {
  
  const [radioResult, setRadioResult] = useState(0);

  const onSubmit = (formData) => {
    setRadioResult(formData.files);
  };

  return (
    <div>
      <div className={s.radioButtonResult}>Selected Language ={" " + radioResult}</div>
      <RadioReduxForm onSubmit={onSubmit} radioInitialValues={radioInitialValues} />
    </div>
  );
};

export default RadioButtons;
