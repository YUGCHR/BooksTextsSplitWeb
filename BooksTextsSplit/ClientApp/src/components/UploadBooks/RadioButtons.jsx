import React, { useState } from "react";
import { connect } from "react-redux";
import s from "./RadioButtons.module.css";

const RadioButtons = ({ uV, cV, i, setRadioResult }) => {
  const [selectedOption, setSelectedOption] = useState("eng");

  const handleOptionChange = (changeEvent) => {
    setSelectedOption(changeEvent.target.value);
  };

  const handleFormSubmit = (formSubmitEvent) => {
    formSubmitEvent.preventDefault();
    console.log("You have submitted:", selectedOption);
    setRadioResult(selectedOption, i);
  };

  const renderRadioButtons = () => {
    return uV.map((d) => {
      let formName = cV.uniqFormName[i];
      return (
        <div>
          <label>
            <input
              type="radio"
              name={formName} // "react-tips"
              value={d.value}
              checked={selectedOption === d.value}
              onChange={handleOptionChange}
              className="form-check-input"
            />
            {d.text}
          </label>
        </div>
      );
    });
  };

  return (
    <div>
      <form onSubmit={handleFormSubmit} className={s.radioFormFields}>
        {renderRadioButtons()}
        <div className={s.radioButtonResult}>
          <button type="submit">Save</button>
        </div>
      </form>
    </div>
  );
};

export default RadioButtons;
