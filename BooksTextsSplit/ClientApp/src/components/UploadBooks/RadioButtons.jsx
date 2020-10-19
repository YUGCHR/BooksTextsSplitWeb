import React, { useState } from "react";
import { connect } from "react-redux";
import { reduxForm } from "redux-form";
import { createField, Input } from "../common/formControls/FormControls";
import { requiredField } from "../common/validators/Validators";
import s from "./RadioButtons.module.css";

let CreateRadioForm = ({ handleSubmit, error, radioFieldsData }) => {
  const radio = radioFieldsData.map((data, i) => {
    return createField(null, "files", Input, [], { type: "radio", value: data.value }, data.text);
  });
  return (
    <form onSubmit={handleSubmit} className={s.radioFormFields}>
      {radio}
      {createField(null, "rememberMe", Input, [], { type: "checkbox" }, "Books Pair selected (2 files)")}
      {error && <div className={s.formSummaryError}>{error}</div>}
      <div>
        <button>Upload</button>
      </div>
    </form>
  );
};

const RadioReduxForm = reduxForm({ form: "upload" })(CreateRadioForm);

const RadioButtons = (props) => {
  let radioFieldsData = [
    { value: "eng", text: "English" },
    { value: "rus", text: "Russian" },
    { value: "other", text: "User lang" },
  ];
  const [radioResult, setRadioResult] = useState(0);

  const onSubmit = (formData) => {
    setRadioResult(formData.files);
  };

  return (
    <div>
      <div className={s.radioButtonResult}>Selected Language ={" " + radioResult}</div>
      <RadioReduxForm onSubmit={onSubmit} radioFieldsData={radioFieldsData} />
    </div>
  );
};

let mapStateToProps = (state) => {
  return {
    captchaUrl: state.auth.captchaUrl,
    isAuth: state.auth.isAuth,
  };
};

export default connect(mapStateToProps, {})(RadioButtons);
