import React, { useState } from "react";
import s from "./Paginator.module.css";
import cn from "classnames";

let Paginator = ({ totalItemsCount, pageSize, currentPage, onPageChanges, portionSize = 10 }) => {
  /* let getClassName = (p) => {
    return currentPage === p ? s.selectedPage : s.pageSelector;
  }; */
  let pagesCount = Math.ceil(totalItemsCount / pageSize);
  let pages = [];

  for (let i = 1; i <= pagesCount; i++) {
    pages.push(i);
  }

  let portionCount = Math.ceil(pagesCount / portionSize); // me - page 220 (6205)
  let [portionNumber, setPortionNumber] = useState(1); // another value does not work
  let leftPortionPageNumber = (portionNumber - 1) * portionSize + 1;
  let rightPortionPageNumber = portionNumber * portionSize;
  //return <div className={cn(styles.paginator)}>
  return (
    <div className={s.paginator}>
      {
        <button
          disabled={portionNumber === 1}
          onClick={() => {
            setPortionNumber(portionNumber - 1);
          }}>
          PREV
        </button>
      }
      {pages
        .filter((p) => p >= leftPortionPageNumber && p <= rightPortionPageNumber)
        .map((p) => {
          return (
            <span
              className={cn({ [s.selectedPage]: currentPage === p }, s.pageNumber)}
              key={p}
              onClick={(e) => {
                onPageChanges(p);
              }}>
              {p}
            </span>
          );
        })}
      {portionCount > portionNumber && (
        <button
          onClick={() => {
            setPortionNumber(portionNumber + 1);
          }}>
          NEXT
        </button>
      )}
      <div className={s.info}>
        {"Total Items Count = "}
        {totalItemsCount}
        {" / Total Pages = "}
        {pagesCount}
      </div>
    </div>
  );
};

export default Paginator;
// className={cn( { [s.selectedPage]: currentPage === p, }, s.pageNumber )}
