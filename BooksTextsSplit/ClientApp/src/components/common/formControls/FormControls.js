import React from "react";
import { Field } from "redux-form";
import s from "./FormControls.module.css";

const FormControl = ({ input, meta: { touched, error }, children }) => {
  const hasError = touched && error;
  return (
    <div className={s.formControl + " " + (hasError ? s.error : "")}>
      <div>{children}</div>
      <div>{hasError && <span>{error}</span>}</div>
    </div>
  );
};

export const Textarea = (props) => {
  const { input, meta, child, ...restProps } = props;
  return (
    <FormControl {...props}>
      <textarea {...input} {...restProps} />
    </FormControl>
  );
};

export const Input = (props) => {
  const { input, meta, child, ...restProps } = props;
  return (
    <FormControl {...props}>
      <input {...input} {...restProps} />
    </FormControl>
  );
};

export const RadioField = (props) => {
  const { input, meta, child, ...restProps } = props;
  debugger;
  return (
    <FormControl {...props}>
      <input {...input} {...restProps} checked={input.value === restProps.valueD} />
    </FormControl>
  );
};

export const Radio = (props) => {
  if (props && props.input && props.options) {
    const renderRadioButtons = (key, index) => {
      return (
        <label className="sans-serif w-100" key={`${index}`} htmlFor={`${props.input.name}-${index}`}>
          <Field
            id={`${props.input.name}`}
            component="input"
            name={props.input.name}
            type="radio"
            value={key}
            className="mh2"
          />
          {props.options[key]}
        </label>
      )
    };
    return (
      <div className="mv3 w-100">
        <div className="b sans-serif pv2 w-100">
          {props.label}
        </div>
        <div>
          {props.options &&
            Object.keys(props.options).map(renderRadioButtons)}
        </div>
      </div>
    );
  }
  return <div></div>
};

export const createField = (placeholder, name, component, validators, props = {}, text = "") => (
  <div className={s.createFormField}>
    <Field placeholder={placeholder} name={name} component={component} validate={validators} {...props} /> {text}
  </div>
);
