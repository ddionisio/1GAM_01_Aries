using UnityEngine;
using System.Collections.Generic;
using fastJSON;

//general purpose criteria
public abstract class Criteria {
	public enum Eval {
		Less,
		Greater,
		Equal,
		NotEqual,
		LessEqual,
		GreaterEqual,
		False,
		True
	}
	
	public Eval eval = Eval.True;
	
	public static Criteria[] LoadCriterias(string data) {
		JSON.Instance.Parameters.UseExtensions = true;
		
		List<Criteria> criterias = JSON.Instance.ToObject<List<Criteria>>(data);
		
		return criterias.ToArray();
	}
	
	public static bool EvaluateCriterias(Criteria[] criterias, Object param) {
		foreach(Criteria criteria in criterias) {
			if(criteria.Evaluate(param)) {
				return true;
			}
		}
		
		return false;
	}
	
	//implements
	
	public abstract bool Evaluate(Object param);
}
