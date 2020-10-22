import React, { useState } from "react";
import { connect } from "react-redux";
import { reduxForm } from "redux-form";
import { createField, Input } from "../common/formControls/FormControls";
import { requiredField } from "../common/validators/Validators";
import s from "./RadioButtons.module.css";
/* uV.map((d, i) => */
/* createField(
          cV.placeholder,
          cV.name,
          cV.component,
          cV.validators,
          { type: cV.type, value: d.value },
          d.text
        ) */
let CreateRadioForm = ({ handleSubmit, error, uV, cV }) => {
  return (
    <form onSubmit={handleSubmit} className={s.radioFormFields}>
      {createField("eng", "radioFieldName", Input, [], { type: "radio", value: "eng" }, "English")}
      {createField("eng", "radioFieldName", Input, [], { type: "radio", value: "rus" }, "Russian")}
      {createField("eng", "radioFieldName", Input, [], { type: "radio", value: "oth" }, "Other")}
      {/* {createField(null, "rememberMe", Input, [], { type: "checkbox" }, "Books Pair selected (2 files)")} */}
      {error && <div className={s.formSummaryError}>{error}</div>}
      <div>
        <button>Choose Language</button>
      </div>
    </form>
  );
};
// checked: d.checked, defaultChecked: d.checked,
// initialValues={(commonValues.name): "eng"} radioFieldName
//, initialValues: { name: "radioFieldName", value: "eng" }
const RadioButtons = ({ formName, uniqValues, commonValues, setRadioResult, index }) => {
  //let features = commonValues.name;
  const RadioReduxForm = reduxForm({ form: formName, initialValues: { radioFieldName: "eng" } })(CreateRadioForm);
  // , initialValues: { radioFieldName0: true }
  /* export default reduxForm({ form: 'simple', // a unique identifier for this form 
    initialValues: { tadmin: true },
  })(SimpleForm); */
  //const [radioResult, setRadioResult] = useState(0);

  const onSubmit = (formData) => {
    setRadioResult(formData[commonValues.name], index); // .radioFieldName
  };

  return (
    <div>
      {/* <div className={s.radioButtonResult}>Selected Language ={" " + radioResult}</div> */}
      <RadioReduxForm onSubmit={onSubmit} uV={uniqValues} cV={commonValues} setRadioResult={setRadioResult} index={index} />
    </div>
  );
};

export default RadioButtons;
