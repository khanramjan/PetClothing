/* eslint-disable @typescript-eslint/no-explicit-any */
import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useCheckoutStore } from "../store/checkoutStore";

const Checkout = () => {
  const navigate = useNavigate();
  const { currentStep } = useCheckoutStore();
  const token = typeof window !== "undefined" ? localStorage.getItem("token") : null;

  useEffect(() => {
    if (!token) navigate("/login");
  }, [token, navigate]);

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold">Checkout - Step {currentStep}</h1>
      <p className="text-gray-600 mt-4">Checkout page - Full implementation coming soon</p>
      <div className="mt-6 p-4 bg-blue-50 rounded border border-blue-200">
        <p className="text-blue-900"> Backend API ready with 6 endpoints</p>
        <p className="text-blue-900"> Frontend store created with Zustand</p>
        <p className="text-blue-900"> 4-step checkout flow structured</p>
        <p className="text-blue-900">Next: Integrate Stripe Elements &amp; payment processing</p>
      </div>
    </div>
  );
};

export default Checkout;
