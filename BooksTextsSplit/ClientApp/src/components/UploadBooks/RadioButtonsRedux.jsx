import React, { useState } from "react";
import { connect } from "react-redux";
import { reduxForm, Field } from "redux-form";
import { createField, Input, Radio, RadioField } from "../common/formControls/FormControls";
import { requiredField } from "../common/validators/Validators";
import s from "./RadioButtons.module.css";
import { formObj } from "./ShowSelectedFiles";

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
      {/* {createField("eng", "buttonEng", Input, [], { type: "radio", value: "eng" }, "English")}
      {createField("rus", "buttonRus", Input, [], { type: "radio", value: "rus" }, "Russian")}
      {createField("oth", "buttonOth", Input, [], { type: "radio", value: "oth" }, "Other")} */}
      {/* {createField(null, "rememberMe", Input, [], { type: "checkbox" }, "Books Pair selected (2 files)")} */}
      {error && <div className={s.formSummaryError}>{error}</div>}
      <div>
        <button>Choose Language</button>
      </div>
    </form>
  );
};

const RadioReduxForm = reduxForm(formObj)(CreateRadioForm);

// checked: d.checked, defaultChecked: d.checked,
// initialValues={(commonValues.name): "eng"} radioFieldName
//, initialValues: { radioFieldName: "eng" }
const RadioButtons = ({ uniqValues, commonValues, setRadioResult, index }) => {
  //let features = commonValues.name;
  //let formName = commonValues.uniqFormName[index];

  // , initialValues: { radioFieldName0: true }
  /* export default reduxForm({ form: 'simple', // a unique identifier for this form 
    initialValues: { tadmin: true },
  })(SimpleForm); */
  //const [radioResult, setRadioResult] = useState(0);

  /* const onSubmit = (formData) => {
    // add return formData !!!        
    setRadioResult(formData, index); // .radioFieldName
  }; */

  return (
    <div>
      {/* <div className={s.radioButtonResult}>Selected Language ={" " + radioResult}</div> */}
      {/* <RadioReduxForm onSubmit={onSubmit} uV={uniqValues} cV={commonValues} setRadioResult={setRadioResult} index={index} /> */}
      {/* <Field name="spiceLevel" label="Spice Level" component={Radio} options={{ mild: "Mild", medium: "Medium", hot: "hot" }} /> */}
    </div>
  );
};

export default RadioButtons;
